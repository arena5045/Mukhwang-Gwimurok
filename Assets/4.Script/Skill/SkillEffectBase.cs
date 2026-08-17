using System;
using System.Collections;

[Serializable]
public abstract class SkillEffectBase
{
    public abstract IEnumerator Execute(SkillContext context);
}

