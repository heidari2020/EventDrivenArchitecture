using EventDrivenArchitecture.Core.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenArchitecture.src.Domain.Events;

public record ProductCreatedEvent(Guid Id, DateTime Createdat) : IDomainEvent;
