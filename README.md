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
var q = context.Query<User>(); //return a IQuery<T> object
```
* Query
```C#
int id = 0;
string name = "lu";
string nullString = null;
q.Where(a => a.Id == id).FirstOrDefault();
q.Where(a => a.Id == 1 && a.Id == id && a.Name == name && a.Name == null && a.Name == nullString).ToList();
q.Where(a => a.Id > 0).OrderBy(a => a.Id).ThenByDesc(a => a.Age).Skip(1).Take(999).ToList();
```
* Join Query
```C#
var q = context.Query<User>();
var q1 = context.Query<User>();
q.InnerJoin(q1, (a, b) => a.Id == b.Id).Select((a, b) => new { A = a, B = b }).ToList();
q.LeftJoin(q.Select(a => new { a.Id, a.Name }), (a, b) => a.Id == b.Id + 1).Select((a, b) => new { A = a, B = b }).ToList();
q.RightJoin(q1, (a, b) => a.Id == b.Id).Select((a, b) => new { A = a, B = b }).ToList();
q.InnerJoin(q1, (a, b) => a.Id == b.Id).LeftJoin(q2, (a, b, c) => a.Name == c.Name).RightJoin(q, (a, b, c, d) => a.Id == d.Id + 1).Select((a, b, c, d) => new { A = a, B = b, C = c, D = d }).ToList();
```
* Group Query
```C#
var q = context.Query<User>();
q.GroupBy(a => a.Age).Having(a => a.Age > 1 && DbFunctions.Count() > 0).Select(a => new { a.Age, Count = DbFunctions.Count(), Sum = DbFunctions.Sum(a.Age), Max = DbFunctions.Max(a.Age), Min = DbFunctions.Min(a.Age), Avg = DbFunctions.Average(a.Age) }).ToList();
```
