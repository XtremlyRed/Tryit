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
/// 表示应用程序上下文接口，继承自 <see cref="IContext"/>，用于扩展应用程序级别的上下文功能。
/// </summary>
public interface IApplicationContext : IContext { }

/// <summary>
/// comment here
///
/// </summary>
public class ApplicationContext : ContextBase, IApplicationContext { }
