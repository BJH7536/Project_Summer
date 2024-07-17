using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHoldable
{
    public void Hold(Player holder) { }

    public void Release(Player holder) { }
}

public abstract class Holdable : MonoBehaviour, IHoldable
{
    public abstract void Hold(Player holder);

    public abstract void Release(Player holder);
}
