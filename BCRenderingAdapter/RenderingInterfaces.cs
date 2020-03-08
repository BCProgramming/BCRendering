using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeCamp.Rendering.Interfaces
{
    //basic RenderingHandler interface. "T" is the owning type- within a project this should be a relatively universally understood type, if additional information is needed about the state of the program for drawing.

    public interface IRenderingHandler<in T>
    {
        void Render(T pOwner, Object pRenderTarget, Object RenderSource, Object Element);
    }
  
    /// <summary>
    /// defines a Rendering Handler. 
    /// </summary>
    /// <typeparam name="TRenderTarget">Type of this rendering form. This is the main input parameter that describes the draw routine- for example, it might be a Drawing.Graphics or even a separate class type.</typeparam>
    /// <typeparam name="TRenderSource">Type of the class instance this Handler is designed to draw. (eg. A Block, Ball, Character, tile, etc.)</typeparam>
    /// <typeparam name="TDataElement">Element type that contains additional data.</typeparam>
    /// <typeparam name="TOwnerType">Owning type definition.</typeparam>
    public interface IRenderingHandler<in TRenderTarget, in TRenderSource, in TDataElement,in TOwnerType> : IRenderingHandler<TOwnerType> where TRenderSource : class
    {
        //rendering handler has pretty much one method- to draw to the appropriate type.
        void Render(TOwnerType pOwner, TRenderTarget pRenderTarget, TRenderSource Source, TDataElement Element);
    }
   
    public sealed class NoDataElement
    {

    }

    //A rendering Provider is a class that accepts a Class Type, and a Data Element Type, and 
    //attempts to give back an appropriate Rendering Handler implementation for that class and element type.
    public interface IRenderingProvider<in T>
    {
        IRenderingHandler<T> GetHandler(Type ClassType, Type DrawType, Type DrawDataType);
    }

    //Concrete base class IRenderingHandler.
    /// <summary>
    /// abstract Rendering base class.
    /// </summary>
    /// <typeparam name="TClassType">The class type of the draw target. (Graphics canvas for example)</typeparam>
    /// <typeparam name="TDrawType">Class type of the object being drawn.</typeparam>
    /// <typeparam name="TDataType">Class type that holds additional data for the operation.</typeparam>
    public abstract class StandardRenderingHandler<TClassType, TDrawType, TDataType,TOwnerType> : IRenderingHandler<TClassType, TDrawType, TDataType,TOwnerType> where TDrawType : class
    {
        public abstract void Render(TOwnerType pOwner, TClassType pRenderTarget, TDrawType Source, TDataType Element);


        public void Render(TOwnerType pOwner, object pRenderTarget, object Element, object ElementData)
        {
            this.Render(pOwner, (TClassType)pRenderTarget, (TDrawType)Element, (TDataType)ElementData);
        }
    }

}
