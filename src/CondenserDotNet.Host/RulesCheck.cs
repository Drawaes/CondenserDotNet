using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CondenserDotNet.Host
{
    public class RulesCheck
    {
        Version _version;
        string[] _patterns;
        Func<IHeaderDictionary,bool> _ex;
        
        public enum VersionComparer
        {
            Max,
            Min,
            Exact
        }

        public RulesCheck(string[] patterns, string version, VersionComparer versionComparison )
        {
            _version = new Version(version);
            _patterns = patterns;
                        
            if(patterns.Contains("*"))
            {
                _ex = headers => true;
            }

            //Otherwise we need to make an expression for each pattern
        }

        public Service[] ServiceList(Service[] availableServices, IHeaderDictionary headers)
        {
            //if(_ex(headers))
            //{
            //    switch(_compareType)
            //    {
            //        case VersionComparer.Exact:
            //            return availableServices.Where(s => s.SupportedVersions.Contains(_version)).ToArray();
            //        case VersionComparer.Max:
            //            return availableServices.Where(s => s.SupportedVersions.Max() < _version).ToArray();
            //        case VersionComparer.Min:
            //            return availableServices.Where(s => s.SupportedVersions.Min() > _version).ToArray();
            //        default:
            //            return new Service[0];
            //    }
            //}
            //else
            //{
                return null;
            //}
            
        }
    }
}
