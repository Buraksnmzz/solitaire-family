using System;

public enum RCValueType
{
    String,
    Number,
    Boolean
}


[Serializable]
public class RCParameterDefinition
{
    public string key;
    public RCValueType valueType;
    public string value;
}