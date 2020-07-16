using System;
using System.Timers;
using System.Net.Http;
using System.Threading.Tasks;
using Hazelcast.Client;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Hazelcast.Core;
using System.Collections;

[Serializable]
public class Metric {

   // public long Id { get; set; }
    public string UserName { get; set; } 
    public string ShimKey { get; set; } 
    public string Endpoint { get; set; } 
    public string Value { get; set; } 
}


