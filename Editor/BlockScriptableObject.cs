using System.Collections.Generic;
using UnityEngine;

namespace SRMI
{
    public class BlockScriptableObject : ScriptableObject
    {
        // Fields

        [SerializeField]
        int m_ObjectLength;

        [SerializeField]
        int m_SkippedObjectLength;

        [SerializeField]
        List<KeyValuePair<Class.PPtr<Class.Object>, string>> m_Container;

        [SerializeField]
        List<KeyValuePair<string,string>> _;


        // Properties

        public int ObjectLength { get { return m_ObjectLength; } set { m_ObjectLength = value; } }

        public int SkippedObjectLength { get { return m_SkippedObjectLength; } set { m_SkippedObjectLength = value; } }

        public List<KeyValuePair<Class.PPtr<Class.Object>, string>> Container { get { return m_Container; } set { m_Container = value; } }


        // Methods

        public BlockScriptableObject()
        {
            Container = new();
        }
    }
}