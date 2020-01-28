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
    public interface IStateRenderingHandler<T> : IRenderingHandler<T>
    {
        void RenderStats(T pOwner, Object pRenderTarget, Object RenderSource, Object Element);
    }
    /// <summary>
    /// defines a Rendering Handler. 
    /// </summary>
    /// <typeparam name="TRenderTarget">Type of this rendering form. This is the main input parameter that describes the draw routine- for example, it might be a Drawing.Graphics or even a separate class type.</typeparam>
    /// <typeparam name="TRenderSource">Type of the class instance this Handler is designed to draw. (eg. A Block, Ball, Character, tile, etc.)</typeparam>
    /// <typeparam name="TDataElement">Element type that contains additional data.</typeparam>
    public interface IRenderingHandler<in TRenderTarget, in TRenderSource, in TDataElement,in TOwnerType> : IRenderingHandler<TOwnerType> where TRenderSource : class
    {
        //rendering handler has pretty much one method- to draw to the appropriate type.
        void Render(TOwnerType pOwner, TRenderTarget pRenderTarget, TRenderSource Source, TDataElement Element);
    }
   
    public sealed class NoDataElement
    {

    }
}
