# XPatchLib - .Net 增量内容 序列化/反序列化 工具
本项目旨在基于 .Net Framework 创建一套 将指定的两个同一类型的对象实例间增量的内容序列化为指定格式文档，也可以将包含增量内容的文档反序列化并附加至原始的对象实例上 的工具。 

## **Example**

让我们看一个如何使用XPatchLib在两个类型相同但内容不同的对象间，创建增量内容以及合并增量内容的例子。
首先，我们定义了一个简单的CreditCard类：
```cs
public class CreditCard
{
    public string CardExpiration { get; set; }
    public string CardNumber { get; set; }
    public override bool Equals(object obj)
    {
        CreditCard card = obj as CreditCard;
        if (card == null)
        {
            return false;
        }
        return string.Equals(this.CardNumber, card.CardNumber) 
            && string.Equals(this.CardExpiration, card.CardExpiration);
    }
}
```
同时创建两个类型相同，但内容不同的CreditCard对象。
```cs
CreditCard card1 = new CreditCard()
{
    CardExpiration = "05/12",
    CardNumber = "0123456789"
};
CreditCard card2 = new CreditCard()
{
    CardExpiration = "05/17",
    CardNumber = "9876543210"
};
```

## **产生增量内容**
使用默认提供的 XML格式文档写入器 XmlTextWriter， 调用XPatchLib.Serializer对两个对象的增量内容进行序列化。

```cs
Serializer serializer = new Serializer(typeof(CreditCard));
string context = string.Empty;
using (MemoryStream stream = new MemoryStream())
{
    var settings = new XmlWriterSettings();
    settings.Encoding = Encoding.UTF8;
    settings.Indent = true;
    using (var xmlWriter = XmlWriter.Create(fs, settings))
    {
         using (var writer = new XmlTextWriter(xmlWriter))
         {
              serializer.Divide(writer, card1, card2);
         }
    }
    using (var stremReader = new StreamReader(stream, settings.Encoding))
    {
        context = stremReader.ReadToEnd();
    }
}
```
经过执行以上代码，context的内容将为：
```xml
<?xml version=""1.0"" encoding=""utf-8""?>
<CreditCard>
  <CardExpiration>05/17</CardExpiration>
  <CardNumber>9876543210</CardNumber>
</CreditCard>
```
通过以上代码，我们实现了两个同类型的对象实例间，增量的序列化。记录了两个对象之间增量的内容。

## **合并增量内容**
下面将介绍如何使用默认提供的 XML格式文档读取器 XmlTextReader， 将已序列化的增量内容附加回原始对象实例，使其与修改后的对象实例形成两个值相同的对象实例。
```cs
CreditCard card3 = null;
Serializer serializer = new Serializer(typeof(CreditCard));
using (var fs = new FileStream(filename, FileMode.Open))
{
     using (var xmlReader = XmlReader.Create(fs))
     {
           using (var reader = new XmlTextReader(xmlReader))
           {
                card3 = (CreditCard)serializer.Combine(reader, card1);
           }
     }
}
```
经过以上代码，可以使新增的 card3 实例的 CardExpiration 属性的值由card1实例中的 "05/12" 变更为增量内容中记录的 "05/17"，CardNumber的值也由card1实例中的"0123456789"变更为了增量内容中记录的"9876543210"。如果使用值比较的方式比较 card3 和 card2 两个实例，会发现这两个实例完全相同。



## **Download**
[Download the latest version.](https://www.nuget.org/packages/XPatchLib/)



## **Documentation**

[Read the documentation.](http://www.cnblogs.com/guqiangjs/p/4616442.html)