-- Lua 5.3+ removed these math functions which are needed by LibSerialize 
if not math.frexp then
  function math.frexp(x)
    if x == 0 then return 0.0,0.0 end
    local e = math.floor(math.log(math.abs(x)) / math.log(2))
    if e > 0 then
      x = x * 2^-e
    else
      x = x / 2^e
    end
    if math.abs(x) >= 1.0 then
      x,e = x/2,e+1
    end
    return x,e
  end
end

if not math.ldexp then
  function math.ldexp(m, e)
    return m * 2 ^ e 
  end
end