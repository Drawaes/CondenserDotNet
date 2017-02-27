using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CondenserTests
{
    public class TagsToRoutesFacts
    {
        private const string UrlPrefix = "urlprefix-";

        [Fact]
        public void TagsWithoutRoute()
        {
            var routes = CondenserDotNet.Core.ServiceUtils.RoutesFromTags(new string[] {$"{UrlPrefix}/this/url/is/cool","thisTagIsnt"});
            Assert.Equal(1, routes.Length);
            Assert.Equal("/this/url/is/cool", routes[0]);
        }

        [Fact]
        public void SlashAddedToTheFront()
        {
            var routes = CondenserDotNet.Core.ServiceUtils.RoutesFromTags(new string[] { $"{UrlPrefix}this/url/is/almost/cool" });
            Assert.Equal("/this/url/is/almost/cool", routes[0]);
        }

        [Fact]
        public void SlashRemovedFromTheBack()
        {
            var routes = CondenserDotNet.Core.ServiceUtils.RoutesFromTags(new string[] { $"{UrlPrefix}this/url/needs/work/" });
            Assert.Equal("/this/url/needs/work", routes[0]);
        }
    }
}
