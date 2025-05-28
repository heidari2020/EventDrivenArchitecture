using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvenDrivenArchitecture.Events;
 
public record MessageProduced(string Content) : INotification;
