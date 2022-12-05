if not WAExporter then
  WAExporter = {}
end

local Private = {}
local WeakAuras = {}
local db = WeakAurasSaved
WeakAuras.BuildInfo = 100002
local time = os.time

-- fields that are handled as special cases when importing
-- mismatch of internal fields is not counted as a difference
Private.internal_fields = {
  uid = true,
  internalVersion = true,
  sortHybridTable = true,
  tocversion = true,
  parent = true,
  controlledChildren = true,
  source = true
}

-- fields that are not included in exported data
-- these represent information which is only meaningful inside the db,
-- or are represented in other ways in exported
Private.non_transmissable_fields = {
  controlledChildren = true,
  parent = true,
  authorMode = true,
  skipWagoUpdate = true,
  ignoreWagoUpdate = true,
  preferToUpdate = true,
  information = {
    saved = true
  }
}

-- For nested groups, we do transmit parent + controlledChildren
Private.non_transmissable_fields_v2000 = {
  authorMode = true,
  skipWagoUpdate = true,
  ignoreWagoUpdate = true,
  preferToUpdate = true,
  information = {
    saved = true
  }
}

local configForLS = {
  errorOnUnserializableType =  false
}

-- Local functions
local decodeB64, GenerateUniqueID
local CompressDisplay, ShowTooltip, TableToString, StringToTable
local RequestDisplay, TransmitError, TransmitDisplay

local bytetoB64 = {
  [0]="a","b","c","d","e","f","g","h",
  "i","j","k","l","m","n","o","p",
  "q","r","s","t","u","v","w","x",
  "y","z","A","B","C","D","E","F",
  "G","H","I","J","K","L","M","N",
  "O","P","Q","R","S","T","U","V",
  "W","X","Y","Z","0","1","2","3",
  "4","5","6","7","8","9","(",")"
}

local B64tobyte = {
  a =  0,  b =  1,  c =  2,  d =  3,  e =  4,  f =  5,  g =  6,  h =  7,
  i =  8,  j =  9,  k = 10,  l = 11,  m = 12,  n = 13,  o = 14,  p = 15,
  q = 16,  r = 17,  s = 18,  t = 19,  u = 20,  v = 21,  w = 22,  x = 23,
  y = 24,  z = 25,  A = 26,  B = 27,  C = 28,  D = 29,  E = 30,  F = 31,
  G = 32,  H = 33,  I = 34,  J = 35,  K = 36,  L = 37,  M = 38,  N = 39,
  O = 40,  P = 41,  Q = 42,  R = 43,  S = 44,  T = 45,  U = 46,  V = 47,
  W = 48,  X = 49,  Y = 50,  Z = 51,["0"]=52,["1"]=53,["2"]=54,["3"]=55,
  ["4"]=56,["5"]=57,["6"]=58,["7"]=59,["8"]=60,["9"]=61,["("]=62,[")"]=63
}

function WeakAuras.GetData(id)
  return id and db.displays[id];
end

do
  local function shouldInclude(data, includeGroups, includeLeafs)
    if data.controlledChildren then
      return includeGroups
    else
      return includeLeafs
    end
  end

  local function Traverse(data, includeSelf, includeGroups, includeLeafs)
    if includeSelf and shouldInclude(data, includeGroups, includeLeafs) then
      coroutine.yield(data)
    end

    if data.controlledChildren then
      for _, child in ipairs(data.controlledChildren) do
        Traverse(WeakAuras.GetData(child), true, includeGroups, includeLeafs)
      end
    end
  end

  local function TraverseLeafs(data)
    return Traverse(data, false, false, true)
  end

  local function TraverseLeafsOrAura(data)
    return Traverse(data, true, false, true)
  end

  local function TraverseGroups(data)
    return Traverse(data, true, true, false)
  end

  local function TraverseSubGroups(data)
    return Traverse(data, false, true, false)
  end

  local function TraverseAllChildren(data)
    return Traverse(data, false, true, true)
  end

  local function TraverseAll(data)
    return Traverse(data, true, true, true)
  end

  local function TraverseParents(data)
    while data.parent do
      local parentData = WeakAuras.GetData(data.parent)
      coroutine.yield(parentData)
      data = parentData
    end
  end

  -- Only non-group auras, not include self
  function Private.TraverseLeafs(data)
    return coroutine.wrap(TraverseLeafs), data
  end

  -- The root if it is a non-group, otherwise non-group children
  function Private.TraverseLeafsOrAura(data)
    return coroutine.wrap(TraverseLeafsOrAura), data
  end

  -- All groups, includes self
  function Private.TraverseGroups(data)
    return coroutine.wrap(TraverseGroups), data
  end

  -- All groups, excludes self
  function Private.TraverseSubGroups(data)
    return coroutine.wrap(TraverseSubGroups), data
  end

  -- All Children, excludes self
  function Private.TraverseAllChildren(data)
    return coroutine.wrap(TraverseAllChildren), data
  end

  -- All Children and self
  function Private.TraverseAll(data)
    return coroutine.wrap(TraverseAll), data
  end

  function Private.TraverseParents(data)
    return coroutine.wrap(TraverseParents), data
  end

  --- Returns whether the data is a group or dynamicgroup
  ---@param data auraData
  ---@return boolean
  function Private.IsGroupType(data)
    return data.regionType == "group" or data.regionType == "dynamicgroup"
  end
end

function GenerateUniqueID()
  -- generates a unique random 11 digit number in base64
  local s = {}
  for i=1,11 do
    tinsert(s, bytetoB64[math.random(0, 63)])
  end
  return table.concat(s)
end
WeakAuras.GenerateUniqueID = GenerateUniqueID

local function stripNonTransmissableFields(datum, fieldMap)
  for k, v in pairs(fieldMap) do
    if type(v) == "table" and type(datum[k]) == "table" then
      stripNonTransmissableFields(datum[k], v)
    elseif v == true then
      datum[k] = nil
    end
  end
end

function CopyTable(template)
  local result = {}
  for k,v in pairs(template) do
    result[k] = v
  end
  return result
end

function CompressDisplay(data, version)
  -- Clean up custom trigger fields that are unused
  -- Those can contain lots of unnecessary data.
  -- Also we warn about any custom code, so removing unnecessary
  -- custom code prevents unnecessary warnings
  for triggernum, triggerData in ipairs(data.triggers) do
    local trigger, untrigger = triggerData.trigger, triggerData.untrigger

    if (trigger and trigger.type ~= "custom") then
      trigger.custom = nil;
      trigger.customDuration = nil;
      trigger.customName = nil;
      trigger.customIcon = nil;
      trigger.customTexture = nil;
      trigger.customStacks = nil;
      if (untrigger) then
        untrigger.custom = nil;
      end
    end
  end

  local copiedData = CopyTable(data)
  local non_transmissable_fields = version >= 2000 and Private.non_transmissable_fields_v2000
                                                       or Private.non_transmissable_fields
  stripNonTransmissableFields(copiedData, non_transmissable_fields)
  copiedData.tocversion = WeakAuras.BuildInfo
  return copiedData;
end

function Private.DisplayToString(id, forChat)
  local data = WeakAuras.GetData(id);
  if(data) then
    data.uid = data.uid or GenerateUniqueID()
    -- Check which transmission version we want to use
    local version = 1421
    for child in Private.TraverseSubGroups(data) do -- luacheck: ignore
      version = 2000
      break;
    end
    local transmitData = CompressDisplay(data, version);
    local transmit = {
      m = "d",
      d = transmitData,
      v = version,
      s = versionString
    };
    if(data.controlledChildren) then
      transmit.c = {};
      local uids = {}
      local index = 1
      for child in Private.TraverseAllChildren(data) do
        if child.uid then
          if uids[child.uid] then
            child.uid = GenerateUniqueID()
          else
            uids[child.uid] = true
          end
        else
          child.uid = GenerateUniqueID()
        end
        transmit.c[index] = CompressDisplay(child, version);
        index = index + 1
      end
    end
    return TableToString(transmit, forChat);
  else
    return "";
  end
end

local compressedTablesCache = {}
function TableToString(inTable, forChat)
  local serialized = LibSerialize:SerializeEx(configForLS, inTable)
  local compressed
  -- get from / add to cache
  if compressedTablesCache[serialized] then
    compressed = compressedTablesCache[serialized].compressed
    compressedTablesCache[serialized].lastAccess = time()
  else
    compressed = LibDeflate:CompressDeflate(serialized, configForDeflate)
    compressedTablesCache[serialized] = {
      compressed = compressed,
      lastAccess = time(),
    }
  end
  -- remove cache items after 5 minutes
  for k, v in pairs(compressedTablesCache) do
    if v.lastAccess < (time() - 300) then
      compressedTablesCache[k] = nil
    end
  end
  local encoded = "!WA:2!"
  if(forChat) then
    encoded = encoded .. LibDeflate:EncodeForPrint(compressed)
  else
    encoded = encoded .. LibDeflate:EncodeForWoWAddonChannel(compressed)
  end
  return encoded
end

function WAExporter.DisplayToString(id)
  return Private.DisplayToString(id, true)
end

return WAExporter.DisplayToString