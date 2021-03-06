﻿// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System.Xml;
using XPatchLib.UnitTest.PetShopModelTests.Models;
#if NUNIT
using NUnit.Framework;

#elif XUNIT
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = XPatchLib.UnitTest.XUnitAssert;
#endif

namespace XPatchLib.UnitTest.ForXml.PetShopModelTests
{
    [TestFixture]
    public class SingleModelTest:TestBase
    {
        [Test]
        [Description("测试两个同一类型的复杂对象间改变值的增量内容是否产生正确，是否能够正确合并，并且合并后值相等")]
        public void TestOrderInfoDivideAndCombine()
        {
            var oriObj = PetShopModelTestHelper.CreateNewOriOrderInfo();
            var changedObj = PetShopModelTestHelper.CreateNewOriOrderInfo();
            var changedContext = string.Empty;

            //更新了一个全新的CreditCard信息，并且重新赋值了UserId属性
            changedObj.UserId = "UserId-3";
            changedObj.CreditCard = new CreditCardInfo("American Express", "0123456789", "12/15")
            {
                CardId = oriObj.CreditCard.CardId
            };

            changedContext = @"<OrderInfo>
  <CreditCard>
    <CardExpiration>12/15</CardExpiration>
    <CardNumber>0123456789</CardNumber>
    <CardType>American Express</CardType>
  </CreditCard>
  <UserId>UserId-3</UserId>
</OrderInfo>";

            //更新了一个全新的CreditCard信息，并且重新赋值了UserId属性
            DoAssert(typeof(OrderInfo), changedContext, oriObj, changedObj, true);
            DoAssert(typeof(OrderInfo), changedContext, oriObj, changedObj, false);

            oriObj = PetShopModelTestHelper.CreateNewOriOrderInfo();
            changedObj = PetShopModelTestHelper.CreateNewOriOrderInfo();
            changedObj.Date = changedObj.Date.AddDays(1);

            changedContext = @"<OrderInfo>
  <Date>" + XmlConvert.ToString(changedObj.Date, XmlDateTimeSerializationMode.RoundtripKind) + @"</Date>
</OrderInfo>";
            //更新OrderInfo中的Date信息(RoundtripKind)
            DoAssert(typeof(OrderInfo), changedContext, oriObj, changedObj, true);
            DoAssert(typeof(OrderInfo), changedContext, oriObj, changedObj, false);

            oriObj = PetShopModelTestHelper.CreateNewOriOrderInfo();
            changedObj = PetShopModelTestHelper.CreateNewOriOrderInfo();
            changedObj.Date = changedObj.Date.AddDays(1);
            changedContext = @"<OrderInfo>
  <Date>" + XmlConvert.ToString(changedObj.Date, XmlDateTimeSerializationMode.Local) + @"</Date>
</OrderInfo>";
            //更新OrderInfo中的Date信息(Local)
            ISerializeSetting setting = new XmlSerializeSetting() {Mode = DateTimeSerializationMode.Local};
            DoAssert(typeof(OrderInfo), changedContext, oriObj, changedObj, true, setting);
            DoAssert(typeof(OrderInfo), changedContext, oriObj, changedObj, false, setting);

            oriObj = PetShopModelTestHelper.CreateNewOriOrderInfo();
            changedObj = PetShopModelTestHelper.CreateNewOriOrderInfo();
            changedObj.Date = changedObj.Date.AddDays(1);
            changedContext = @"<OrderInfo>
  <Date>" + XmlConvert.ToString(changedObj.Date, XmlDateTimeSerializationMode.Unspecified) + @"</Date>
</OrderInfo>";
            //更新OrderInfo中的Date信息(Unspecified)
            setting = new XmlSerializeSetting() { Mode = DateTimeSerializationMode.Unspecified };
            DoAssert(typeof(OrderInfo), changedContext, oriObj, changedObj, true, setting);
            DoAssert(typeof(OrderInfo), changedContext, oriObj, changedObj, false, setting);

            oriObj = PetShopModelTestHelper.CreateNewOriOrderInfo();
            changedObj = PetShopModelTestHelper.CreateNewOriOrderInfo();
            var item = new LineItemInfo("22", "NewLineItemInfo", 23, 34, 45m);
            changedObj.LineItems = new[] {item};

            changedContext = @"<OrderInfo>
  <LineItems>
    <LineItemInfo Action=""Add"">
      <ItemId>" + item.ItemId + @"</ItemId>
      <Line>" + item.Line + @"</Line>
      <Name>" + item.Name + @"</Name>
      <Price>" + item.Price + @"</Price>
      <Quantity>" + item.Quantity + @"</Quantity>
    </LineItemInfo>
  </LineItems>
</OrderInfo>";
            //增加一个LineItemInfo
            DoAssert(typeof(OrderInfo), changedContext, oriObj, changedObj, true);
            DoAssert(typeof(OrderInfo), changedContext, oriObj, changedObj, false);

            oriObj = PetShopModelTestHelper.CreateNewOriOrderInfo();
            changedObj = PetShopModelTestHelper.CreateNewOriOrderInfo();
            oriObj.LineItems = new[] {new LineItemInfo("22", "NewLineItemInfo", 23, 34, 45m)};
            changedObj.LineItems = new[] {new LineItemInfo("22", "ChangedLineItemInfo", 23, 45, 45m)};

            changedContext = @"<OrderInfo>
  <LineItems>
    <LineItemInfo ItemId=""" + oriObj.LineItems[0].ItemId + @""">
      <Name>" + changedObj.LineItems[0].Name + @"</Name>
      <Quantity>" + changedObj.LineItems[0].Quantity + @"</Quantity>
    </LineItemInfo>
  </LineItems>
</OrderInfo>";
            //编辑首个LineItemInfo
            DoAssert(typeof(OrderInfo), changedContext, oriObj, changedObj, true);
            DoAssert(typeof(OrderInfo), changedContext, oriObj, changedObj, false);

            oriObj = PetShopModelTestHelper.CreateNewOriOrderInfo();
            changedObj = PetShopModelTestHelper.CreateNewOriOrderInfo();
            oriObj.LineItems = new[]
            {
                new LineItemInfo("11", "NewLineItemInfo1", 123, 134, 145m),
                new LineItemInfo("22", "NewLineItemInfo2", 223, 234, 245m),
                new LineItemInfo("33", "NewLineItemInfo3", 323, 334, 345m)
            };
            changedObj.LineItems = new[]
            {
                new LineItemInfo("11", "NewLineItemInfo1", 123, 134, 145m),
                new LineItemInfo("22", "NewLineItemInfo2", 223, 234, 245m),
                new LineItemInfo("33", "NewLineItemInfo3", 323, 334, 345m)
            };

            changedObj.LineItems[1].Name = "ChangedLineItemInfo2";
            changedObj.LineItems[1].Price = 245.2222m;
            changedObj.LineItems[1].Quantity = 2222;

            changedContext = @"<OrderInfo>
  <LineItems>
    <LineItemInfo ItemId=""" + oriObj.LineItems[1].ItemId + @""">
      <Name>" + changedObj.LineItems[1].Name + @"</Name>
      <Price>" + changedObj.LineItems[1].Price + @"</Price>
      <Quantity>" + changedObj.LineItems[1].Quantity + @"</Quantity>
    </LineItemInfo>
  </LineItems>
</OrderInfo>";
            //编辑非首个LineItemInfo
            DoAssert(typeof(OrderInfo), changedContext, oriObj, changedObj, true);
            DoAssert(typeof(OrderInfo), changedContext, oriObj, changedObj, false);

            oriObj = PetShopModelTestHelper.CreateNewOriOrderInfo();
            changedObj = PetShopModelTestHelper.CreateNewOriOrderInfo();
            oriObj.LineItems = new[]
            {
                new LineItemInfo("11", "NewLineItemInfo1", 123, 134, 145m),
                new LineItemInfo("22", "NewLineItemInfo2", 223, 234, 245m),
                new LineItemInfo("33", "NewLineItemInfo3", 323, 334, 345m)
            };
            changedObj.LineItems = new[]
            {
                new LineItemInfo("11", "NewLineItemInfo1", 123, 134, 145m),
                new LineItemInfo("33", "NewLineItemInfo3", 323, 334, 345m)
            };

            changedContext = @"<OrderInfo>
  <LineItems>
    <LineItemInfo Action=""Remove"" ItemId=""" + oriObj.LineItems[1].ItemId + @""" />
  </LineItems>
</OrderInfo>";
            //删除非首个LineItemInfo
            DoAssert(typeof(OrderInfo), changedContext, oriObj, changedObj, true);
            DoAssert(typeof(OrderInfo), changedContext, oriObj, changedObj, false);
        }
    }
}