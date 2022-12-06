using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WeakAurasInterface.Core.Models;

/// <summary>
///     Represents a single WeakAura (aura, group, etc.)
/// </summary>
public class WeakAuraDisplay
{
    private List<WeakAuraDisplay>? _children;
    private WeakAuraDisplay? _parent;

    public WeakAuraDisplay(string id)
    {
        Id = id;
    }

    public string Id { get; }

    public ReadOnlyCollection<WeakAuraDisplay>? Children => _children?.AsReadOnly();

    public WeakAuraDisplay? Parent
    {
        get => _parent;
        set
        {
            if (_parent == value)
                return;

            //parent is getting removed
            if (value == null)
            {
                _parent?.RemoveChild(this);
                return;
            }

            //parent is changed
            if (_parent != null)
            {
                _parent.RemoveChild(this);
                value.AppendChild(this);
                return;
            }

            //initial set
            value.AppendChild(this);
        }
    }

    public bool AppendChild(WeakAuraDisplay child)
    {
        if (_children != null && _children.Contains(child))
            return false;
        child._parent?.RemoveChild(child);
        _children ??= new List<WeakAuraDisplay>();
        _children.Add(child);
        child._parent = this;
        return true;
    }

    public bool RemoveChild(WeakAuraDisplay child)
    {
        if (_children == null || !_children.Contains(child))
            return false;
        _children.Remove(child);
        child._parent = null;
        return true;
    }
}