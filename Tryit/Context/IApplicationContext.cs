using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tryit;

/// <summary>
/// Defines a context that provides access to application-level services and state.
/// </summary>
/// <remarks>Implementations of this interface typically expose information and operations relevant to the
/// application's lifecycle, configuration, or shared resources. This interface extends <see cref="IContext"/>, and may
/// be used to obtain context-specific data or services within the application.</remarks>
public interface IApplicationContext : IContext { }

/// <summary>
/// Provides contextual information and services for an application, supporting operations that require access to
/// application-level state or configuration.
/// </summary>
/// <remarks>ApplicationContext extends ContextBase to offer additional functionality specific to application-wide
/// scenarios. It is typically used to share resources, configuration, or state across different components within an
/// application. Implementations may vary depending on the application's requirements.</remarks>
public class ApplicationContext : ContextBase, IApplicationContext { }
