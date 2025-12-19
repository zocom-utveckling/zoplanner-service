using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace zoplannerservice.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EventType
{
    NEW_ASSIGNMENT
}

