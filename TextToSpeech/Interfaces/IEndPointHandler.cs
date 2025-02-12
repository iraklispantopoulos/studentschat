using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextToSpeech.Interfaces
{
    internal interface IEndPointHandler<EndPointData,Response> where EndPointData : class
    {
        Task<Response> Handle(EndPointData data);
    }
}
