# Chloe
Chloe is a lightweight Object/Relational Mapping (ORM) library.
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
* Sql Query
```C#
context.SqlQuery<User>("select Id as Id,Name as Name,Age as Age from Users where Name=@name", DbParam.Create("@name", "lu")).ToList();
context.SqlQuery<int>("select Id from Users").ToList();
```
* Aggregate Function
```C#
var q = context.Query<User>();
q.Select(a => DbFunctions.Count()).First();
q.Select(a => new { Count = DbFunctions.Count(), LongCount = DbFunctions.LongCount(), Sum = DbFunctions.Sum(a.Age), Max = DbFunctions.Max(a.Age), Min = DbFunctions.Min(a.Age), Average = DbFunctions.Average(a.Age) }).First();

var count = q.Count();
var longCount = q.LongCount();
var sum = q.Sum(a => a.Age);
var max = q.Max(a => a.Age);
var min = q.Min(a => a.Age);
var avg = q.Average(a => a.Age);
```
* Method
```C#
var q = context.Query<User>();
var space = new char[] { ' ' };
DateTime startTime = DateTime.Now;
DateTime endTime = DateTime.Now.AddDays(1);
q.Select(a => new
{
    Id = a.Id,

    String_Length = a.Name.Length,
    Substring = a.Name.Substring(0),
    Substring1 = a.Name.Substring(1),
    Substring1_2 = a.Name.Substring(1, 2),
    ToLower = a.Name.ToLower(),
    ToUpper = a.Name.ToUpper(),
    IsNullOrEmpty = string.IsNullOrEmpty(a.Name),
    Contains = (bool?)a.Name.Contains("s"),
    Trim = a.Name.Trim(),
    TrimStart = a.Name.TrimStart(space),
    TrimEnd = a.Name.TrimEnd(space),
    StartsWith = (bool?)a.Name.StartsWith("s"),
    EndsWith = (bool?)a.Name.EndsWith("s"),

    SubtractTotalDays = endTime.Subtract(startTime).TotalDays,
    SubtractTotalHours = endTime.Subtract(startTime).TotalHours,
    SubtractTotalMinutes = endTime.Subtract(startTime).TotalMinutes,
    SubtractTotalSeconds = endTime.Subtract(startTime).TotalSeconds,
    SubtractTotalMilliseconds = endTime.Subtract(startTime).TotalMilliseconds,

    Now = DateTime.Now,
    UtcNow = DateTime.UtcNow,
    Today = DateTime.Today,
    Date = DateTime.Now.Date,
    Year = DateTime.Now.Year,
    Month = DateTime.Now.Month,
    Day = DateTime.Now.Day,
    Hour = DateTime.Now.Hour,
    Minute = DateTime.Now.Minute,
    Second = DateTime.Now.Second,
    Millisecond = DateTime.Now.Millisecond,

    Int_Parse = int.Parse("1"),
    Int16_Parse = Int16.Parse("11"),
    Long_Parse = long.Parse("2"),
    Double_Parse = double.Parse("3"),
    Float_Parse = float.Parse("4"),
    Decimal_Parse = decimal.Parse("5"),
    Guid_Parse = Guid.Parse("D544BC4C-739E-4CD3-A3D3-7BF803FCE179"),

    Bool_Parse = bool.Parse("1"),
    DateTime_Parse = DateTime.Parse("2014-1-1"),

    B = a.Age == null ? false : a.Age > 1,
}).ToList();
```
* Insert
```C#
var id = context.Insert<User>(() => new User() { Name = "lu", NickName = "so", Age = 18, Gender = Gender.Man, OpTime = DateTime.Now });//return the key value
User user = context.Insert(new User() { Name = "lu", NickName = "so", Age = 18, Gender = Gender.Man, ByteArray = new byte[] { 1, 2 }, OpTime = DateTime.Now });
```
* Update
```C#
context.Update<User>(a => new User() { Name = a.Name, Age = a.Age + 1, Gender = Gender.Man, OpTime = DateTime.Now }, a => a.Name == "lu");

User user = new User() { Id = 1, Name = "lu", Age = 18, Gender = Gender.Man };
context.Update(user);//update all columns

context.TrackEntity(user);//track entity on the context
user.Name = user.Name + "1";
context.Update(user);//just update the column 'Name'
```
* Delete
```C#
context.Delete<User>(a => a.Id == 1 || a.Age == null);
context.Delete(new User() { Id = 1 });
```
