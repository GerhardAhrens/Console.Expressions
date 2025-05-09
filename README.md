# PropertyInfo, Attribute und Expression

![NET](https://img.shields.io/badge/NET-8.0-green.svg)
![License](https://img.shields.io/badge/License-MIT-blue.svg)
![VS2022](https://img.shields.io/badge/Visual%20Studio-2022-white.svg)
![Version](https://img.shields.io/badge/Version-1.0.2025.0-yellow.svg)]

An Hand von Beispielen zum generieren von SQL Anweisungen soll der Umgang mit PropertyInfo, Attributen und Expression gezeigt werden.

## Festlegen von Custom Attribute
```csharp
[DataTable("TAB_Contact")]
public class Contact
{
    public Contact(string name, int age)
    {
        this.Id = Guid.NewGuid();
        this.Name = name;
        this.Age = age;
    }

    [PrimaryKey]
    [TableColumn(SQLiteDataType.Guid)]
    public Guid Id { get; set; }

    [TableColumn(SQLiteDataType.Text, 50)]
    public string Name { get; }

    [TableColumn(SQLiteDataType.Integer)]
    public int Age { get; }
}
```

## Auslesen von Custom Attribute
### Rückgabe Tabellenname (string)
```csharp
private string DataTableAttributes()
{
    string tableName = string.Empty;
    tableName = typeof(TEntity).GetAttributeValue((DataTableAttribute table) => table.TableName);
    if (string.IsNullOrEmpty(tableName) == true)
    {
        tableName = typeof(TEntity).Name;
    }

    return tableName;
}
```

### Rückgabe einer Liste von DataColumn
```csharp
private IEnumerable<TableColumnAttribute> CustomerAttributesTableColumn()
{
    IEnumerable<TableColumnAttribute> obj = null;
    obj = typeof(TEntity).GetProperties()
        .SelectMany(p => p.GetCustomAttributes())
        .OfType<TableColumnAttribute>()
        .AsParallel();

    return obj;
}
```

### Rückgabe einer Liste von PrimaryKey
```csharp
private List<string> CustomerAttributesPK()
{
    List<string> obj = null;
    obj = typeof(TEntity).GetProperties()
        .SelectMany(p => p.GetCustomAttributes())
        .OfType<PrimaryKeyAttribute>()
        .AsParallel()
        .Select(s => s.ColumnName)
        .ToList();

    return obj;
}
```