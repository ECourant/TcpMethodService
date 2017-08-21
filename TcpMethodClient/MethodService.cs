using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
namespace TcpMethodClient
{
    public abstract class MethodService
    {
        private string ServiceName { get; set; }



        protected MethodService()
        {
            MethodServiceSettings SettingsAttribute = this.GetType().GetCustomAttribute<MethodServiceSettings>();
            if (SettingsAttribute != null)
            {
                if (!string.IsNullOrWhiteSpace(SettingsAttribute.ServiceName))
                    this.ServiceName = SettingsAttribute.ServiceName;
                else
                    this.ServiceName = this.GetType().Name;
            }
            else
                this.ServiceName = this.GetType().Name;
            Console.WriteLine($"SERVICE CREATED [{this.ServiceName}]");
        }


#if NET45
        protected void Invoke()
        {
            this._Invoke();
        }
        protected void Invoke(params object[] Parameters)
        {
            this._Invoke(Parameters);
        }
        protected T Invoke<T>()
        {
            return this._Invoke();
        }
        protected T Invoke<T>(params object[] Parameters)
        {
            return this._Invoke(Parameters);
        }
#elif NETCE
        protected void Invoke(string Method)
        {
            this._Invoke(Method, null);
        }
        protected void Invoke(string Method, params object[] Parameters)
        {
            this._Invoke(Method, Parameters);
        }
        protected T Invoke<T>(string Method)
        {
            return this._Invoke(Method, null);
        }
        protected T Invoke<T>(string Method, params object[] Parameters)
        {
            return this._Invoke(Method, Parameters);
        }
#endif





#if NET45
        private dynamic _Invoke(params object[] Arguments)
#elif NETCE
        private dynamic _Invoke(string MethodName, object[] Arguments)
#endif
        {
            MethodBase Method = null;
            try
            {
#if NET45
                Method = new StackTrace().GetFrames().Where(Frame => Frame.GetMethod().GetCustomAttribute<TcpMethod>() != null).FirstOrDefault().GetMethod();
#elif NETCE
                Method = this.GetType().GetMethods().Where(MethodItem => MethodItem.Name == MethodName && MethodItem.GetCustomAttribute<TcpMethod>() != null).FirstOrDefault();
#endif
            }
            catch (ArgumentNullException)
            {
                throw new InvalidOperationException("Error, the method you are trying to execute with Invoke is not a valid TcpMethod!");
            }
            if (Method == null)
            {
                throw new InvalidOperationException("Error, the method you are trying to execute with Invoke is not a valid TcpMethod!");
            }
            TcpMethod MethodAttribute = Method.GetCustomAttribute<TcpMethod>();
#if NET45
            string MethodName = Method.Name;
#endif
            MRequest Request = new MRequest()
            {
                Method = MethodName,
                Service = ServiceName,
                Arguments = Arguments,
                RequestID = Guid.NewGuid(),
                RequestType = ERequestType.Invoke,
                ClientID = default(Guid),
                Headers = new Dictionary<string, string>()
            };
            MResponse Response = this._InvokeHandler(Request);
            if (Response.Exception != null)
                throw Response.Exception;
            else
                return Response.Response;
        }
        private MethodInvokeHandler _InvokeHandler;



        internal void _Bind(MethodInvokeHandler Handler)
        {
            this._InvokeHandler = Handler;
        }
    }
}
