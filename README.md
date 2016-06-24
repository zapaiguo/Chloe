# Chloe
Chloe is an Object/Relational Mapping (ORM) library.
# Usage
* Entity
```C#
public enum Gender
{
    Man = 1,
    Woman
}

[TableAttribute("Users")]
public class User
{
    [Column(IsPrimaryKey = true)]
    [AutoIncrementAttribute]
    public int Id { get; set; }
    public string Name { get; set; }
    public string NickName { get; set; }
    public Gender? Gender { get; set; }
    public int? Age { get; set; }
    public DateTime? OpTime { get; set; }
    public Byte[] ByteArray { get; set; }
}
```
* DbContext
```C#
MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);
var q = context.Query<User>();
```
* Query
```C#
int id = 0;
string name = "lu";
string nullString = null;
q.Where(a => a.Id == 1 && a.Id == id && a.Name == name && a.Name == null && a.Name == nullString).ToList();
