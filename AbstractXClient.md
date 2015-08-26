```cs

internal class XResourceClient : AbstractXClient<EStoreKind>
{
public override Guid UserUid
{
get { return Guid.Empty; }
}

protected override IEnumerable<KeyValuePair<EStoreKind, EStoreKind>> GetAbstractRootKindMap()
{
yield break;
}

protected override int KindToInt(EStoreKind _kind)
{
return (int) _kind;
}

protected override EStoreKind IntToKind(int _kind)
{
return (EStoreKind) _kind;
}

protected override void ObjectReleased(Guid _uid, EStoreKind _kind)
{
}
}
```