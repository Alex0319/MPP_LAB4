using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Logger;

namespace AopTarget
{
    [Log]
    public class TargetAopClass
    {
        public TargetAopClass(int parameter)
        {
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            Type declaringType = currentMethod.DeclaringType;
            LogAttribute logAttribute = (LogAttribute)Attribute.GetCustomAttribute(declaringType, typeof(LogAttribute));
            logAttribute.OnCallMethod(currentMethod, new Dictionary<string, object>
        	{
		        {
			        "parameter",
		    	    parameter
		        }
	        });
        }

        public void First()
        {

        }

        public int Second(int parameter1, object parameter2)
        {
            return 10;
        }

    }
}
