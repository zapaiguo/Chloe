# Chloe
Chloe is an Object/Relational Mapping (ORM) library.
# Usage
* Query
```C#
int id = 0;
string name = "lu";
string nullString = null;
q.Where(a => a.Id == 1 && a.Id == id && a.Name == name && a.Name == null && a.Name == nullString).ToList();
