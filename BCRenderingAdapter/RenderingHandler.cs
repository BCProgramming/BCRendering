using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BASeCamp.Rendering.Interfaces;

namespace BASeCamp.Rendering
{
    public class RenderAbstractionException : Exception
    {
        public RenderAbstractionException(String pMessage) : base(pMessage)
        {

        }
    }

    public class RenderingProvider<TOwnerType> : IRenderingProvider<TOwnerType>
    {
        public static RenderingProvider<TOwnerType> Static = new RenderingProvider<TOwnerType>();
        private Dictionary<Type, Dictionary<Type, IRenderingHandler<TOwnerType>>> handlerLookup = new Dictionary<Type, Dictionary<Type, IRenderingHandler<TOwnerType>>>();
        bool InitProviderDictionary = false;

        private void AddTaggedHandlers(Assembly Source)
        {
            foreach (var iterate in Source.GetTypes())
            {
                if (iterate.IsClass)
                {
                    if (!iterate.IsAbstract)
                    {
                        if (iterate.Name.Contains("State")) {; }
                        foreach (var findattr in iterate.GetCustomAttributes<RenderingHandlerAttribute>())
                        {
                            if (!handlerLookup.ContainsKey(findattr.CanvasType))
                            {
                                handlerLookup.Add(findattr.CanvasType, new Dictionary<Type, IRenderingHandler<TOwnerType>>());
                            }
                            //we need to construct a class instance to add, then add it to the dictionary.
                            if (!handlerLookup[findattr.CanvasType].ContainsKey(findattr.DrawType))
                            {
                                var handler = (IRenderingHandler<TOwnerType>)Activator.CreateInstance(iterate);
                                handlerLookup[findattr.CanvasType].Add(findattr.DrawType, handler);
                            }
                        }
                    }
                }
            }
        }
        public Assembly FindCaller()
        {
            return Assembly.GetEntryAssembly();
            StackTrace st = new StackTrace();
            StackFrame[] frames = st.GetFrames();
            for(int i=0;i<frames.Length;i++)
            {
                if(frames[i].GetMethod().DeclaringType.Assembly != Assembly.GetAssembly(GetType()))
                {
                    return frames[i].GetMethod().DeclaringType.Assembly;
                }
            }
            return null;
        }
        public IRenderingHandler<TOwnerType> GetHandler(Type ClassType, Type DrawType, Type DrawDataType)
        {
            if (!InitProviderDictionary)
            {


                InitProviderDictionary = true;
                handlerLookup = new Dictionary<Type, Dictionary<Type, IRenderingHandler<TOwnerType>>>();
                AddTaggedHandlers(FindCaller());


            }
            if (handlerLookup.ContainsKey(ClassType))
            {
                if (handlerLookup[ClassType].ContainsKey(DrawType))
                {
                    return handlerLookup[ClassType][DrawType];
                }
                //no? OK, let's try that again- we want to allow for base classes as well though.
                Type Deepest = null;
                int DeepestDepth = 0;
                foreach (var searchtype in handlerLookup[ClassType].Keys)
                {
                    if (searchtype.IsAssignableFrom(DrawType))
                    {
                        var CurrentDepth = GetDerivationDepth(searchtype);
                        if (Deepest == null ||  CurrentDepth > DeepestDepth)
                        {
                            Deepest = searchtype;
                            DeepestDepth = CurrentDepth;
                        }

                    }
                }
                if(Deepest==null)
                {
                    throw new RenderAbstractionException($"No Available Renderer for Type {DrawType.Name} On Canvas Type {ClassType.Name}");
                }
                //after doing the work to find it, let's implicitly slap it into the lookup dictionary, so later lookups are faster.
                var returnresult = handlerLookup[ClassType][Deepest];
                handlerLookup[ClassType].Add(DrawType, returnresult);
                return returnresult;
            }



            return null;
        }
        private int GetDerivationDepth(Type CheckType)
        {
            if (CheckType.BaseType == typeof(Object)) return 0;
            return 1 + GetDerivationDepth(CheckType.BaseType);
        }
        public void DrawElement(TOwnerType pOwner, Object Target, Object Element, Object ElementData)
        {
            var Handler = GetHandler(Target.GetType(), Element.GetType(), ElementData.GetType());
            if (Handler == null)
            {
                throw new RenderAbstractionException("Type " + Element.GetType().Name + " Does not have a rendering provider for type " + Target.GetType().Name);
            }
            Handler.Render(pOwner, Target, Element, ElementData);
        }
       
        /// <summary>
        /// "Extended Info" is intended to allow instance objects- eg game items and objects that can be drawn- to store any data that needs to differ between providers. For example it can storee
        /// stuff in one class for GDI+ and then another for SkiaSharp.
        /// </summary>
        public Dictionary<Object, ExtendedData> extendedInfo = new Dictionary<object, ExtendedData>();
        public ExtendedData GetExtendedData(Type DrawType)
        {
            if (extendedInfo.ContainsKey(DrawType))
            {
                return extendedInfo[DrawType];
            }
            else
            {
                extendedInfo.Add(DrawType, new ExtendedData());
                return extendedInfo[DrawType];
            }
        }
        public Object GetExtendedData(Type DrawType, Object Instance, Func<Object, Object> BuildDataInstance = null)
        {
            ExtendedData data = GetExtendedData(DrawType);
            var result = data.GetPerElementData(Instance, BuildDataInstance);
            return result;
        }
        //public ExtendedData extendedInfo = new ExtendedData();
    }
    public class ExtendedData
    {
        //"per element data" is defined per element (the things being drawn, to be clear). Per state instance, per block instance, etc.
        // basically these can be used to store additional render handler specific fields/properties, for example it can be used to cache images or bitmaps or whatever from
        // one call to the next.

        private static System.Runtime.CompilerServices.ConditionalWeakTable<Object, Object> _extendedData = new System.Runtime.CompilerServices.ConditionalWeakTable<Object, Object>();
        /// <summary>
        /// Retrieves the instance information for the provided instance, or null of there is no current instance information.
        /// </summary>
        /// <param name="Item"></param>
        /// <returns></returns>
        public Object GetPerElementData(Object Item)
        {
            if (_extendedData.TryGetValue(Item, out Object result))
            {
                //we got a result- return it.
                return result;
            }
            else
            {
                return null;
            }
        }
        public Object GetPerElementData(Object Item, Func<Object, Object> NewFunc)
        {
            lock (_extendedData)
            {
                if (_extendedData.TryGetValue(Item, out Object result))
                {
                    //we got a result- return it.
                    return result;
                }
                else
                {
                    Object generatedinstance = NewFunc(Item);
                    _extendedData.Add(Item, generatedinstance);
                    return generatedinstance;
                }
            }
        }
        public void SetPerElementData(Object Item, Object Value)
        {
            _extendedData.Add(Item, Value);
        }
    }
    public class RenderingHandlerAttribute : Attribute
    {
        public Type DrawType { get; set; }
        public Type CanvasType { get; set; }
        public Type DrawParameterType { get; set; }
        public RenderingHandlerAttribute(Type pDrawType, Type pCanvasType, Type pDrawParameterType)
        {
            DrawType = pDrawType;
            CanvasType = pCanvasType;
            DrawParameterType = pDrawParameterType;
        }
    }
}
