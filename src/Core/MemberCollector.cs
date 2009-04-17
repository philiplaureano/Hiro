using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Hiro
{
    /// <summary>
    /// Represents a type that can determine the target member from a given implementation instance.
    /// </summary>
    public class MemberCollector
    {
        private List<ConstructorInfo> _constructors = new List<ConstructorInfo>();
        private List<PropertyInfo> _properties = new List<PropertyInfo>();
        
        /// <summary>
        /// Adds a member to the existing list of members.
        /// </summary>
        /// <typeparam name="TMember">The member type.</typeparam>
        /// <param name="member">The member to be added.</param>
        public void AddMember<TMember>(TMember member)
        {
            if (member == null)
                throw new ArgumentNullException("member");

            if (member is ConstructorInfo)
                _constructors.Add(member as ConstructorInfo);

            if (member is PropertyInfo)
                _properties.Add(member as PropertyInfo);
        }

        /// <summary>
        /// Gets the value indicating the constructors collected by this type.
        /// </summary>
        /// <value>The list of constructors.</value>
        public IEnumerable<ConstructorInfo> Constructors
        {
            get
            {
                return _constructors;
            }
        }

        /// <summary>
        /// Gets the value indicating the properties collected by this type.
        /// </summary>
        /// <value>The list of properties.</value>
        public IEnumerable<PropertyInfo> Properties
        {
            get
            {
                return _properties;
            }
        }        
    }
}
