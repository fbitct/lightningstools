using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace MFDExtractor.Networking
{
    internal class ObjectStore
    {
        private Dictionary<string, object> _rawObjects = new Dictionary<string, object>(); //raw objects that can be transported across the network
        private Dictionary<string, object> _objectLocks = new Dictionary<string, object>(); //lock per object prevents modifying object while being serialized
        private Dictionary<string, int> _preSerializationHashcodes = new Dictionary<string, int>(); //stores pre-serialization hashcodes for serialized objects so that we can tell if a new version exists
        private Dictionary<string, byte[]> _serializedObjects=new Dictionary<string,byte[]>(); //caches serialized objects so we don't have to perform serialization over and over again
        public ObjectStore():base()
        {
        }
        public object GetLockForObject(string objectName)
        {
            object lockObject = _objectLocks.ContainsKey(objectName) ? _objectLocks[objectName] : null;
            if (lockObject == null)
            {

                lockObject = new object();
                _objectLocks.Add(objectName, lockObject);
            }
            return lockObject;

        }
        public object GetRawObject(string objectName)
        {
            if (_rawObjects.ContainsKey(objectName))
            {
                return _rawObjects[objectName];
            }
            else
            {
                return null;
            }
        }
        public void StoreRawObject(string objectName, object obj)
        {
            object objectLock = GetLockForObject(objectName);
            lock (objectLock)
            {
                if (_rawObjects.ContainsKey(objectName))
                {
                    _rawObjects[objectName] = objectName;
                }
                else
                {
                    _rawObjects.Add(objectName, obj);
                }

            }
        }
        public byte[] GetSerializedObject(string objectName)
        {
            if (!Extractor.GetInstance().Running)
            {
                return null;
            }

            object lockObject = GetLockForObject(objectName);
            lock (lockObject)
            {
                //retrieve the current unserialized (raw) object
                object rawObject = GetRawObject(objectName);
                if (rawObject == null) return null; //if it's NULL, then we don't have a value for this object now

                //see if we've ever serialized this object before, and if so, retrieve the latest cached value of the serialized object
                byte[] serializedObject = _serializedObjects.ContainsKey(objectName) ? _serializedObjects[objectName] : null;
                int preSerializationHashcode = _preSerializationHashcodes.ContainsKey(objectName) ? _preSerializationHashcodes[objectName] : 0;

                if (preSerializationHashcode == rawObject.GetHashCode())
                {
                    return serializedObject; //if the latest serialization was of an object having the same hashcode as the current raw object instance, then we don't bother serializing it again
                }

                //here, we either don't have a cached serialized version or the hashcode doesn't match 
                //so we need to re-serialize and  cache the serialized version
                serializedObject = SerializeObject(rawObject);
                if (_serializedObjects.ContainsKey(objectName))
                {
                    _serializedObjects[objectName] = serializedObject;
                }
                else
                {
                    _serializedObjects.Add(objectName, serializedObject);
                }

                //store the pre-serialized object's hashcode for later comparisons.  
                if (_preSerializationHashcodes.ContainsKey(objectName))
                {
                    _preSerializationHashcodes[objectName] = serializedObject.GetHashCode();
                }
                else
                {
                    _preSerializationHashcodes.Add(objectName, serializedObject.GetHashCode());
                }

                return serializedObject;
            }
        }
        public virtual byte[] SerializeObject(object toSerialize)
        {
            if (toSerialize == null)
            {
                return null;
            }
            else
            {
                byte[] bytes = null;
                using (MemoryStream ms = new MemoryStream(1024))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, toSerialize);
                    ms.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    bytes = new byte[ms.Length];
                    ms.Read(bytes, 0, (int)bytes.Length);
                    ms.Close();
                }
                return bytes;
            }
        }
    }
}
