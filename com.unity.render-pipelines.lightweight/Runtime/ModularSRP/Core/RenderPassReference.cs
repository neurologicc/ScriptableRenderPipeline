using UnityEngine;

public interface IRenderPassReference
{
    void SetValue(object value);
}

public class RenderPassReference<T> : IRenderPassReference
{
    public T Value;

    public RenderPassReference()
    {
        Value = default(T);
    }

    public void SetValue(object value)
    {
        if(value.GetType() == typeof(T))
        {
            Value = (T)value;
        }
        else
        {
            Debug.Log("Cannot assign RenderPassReference<" + typeof(T) + "> a " + value.GetType());
        }
    }
}
