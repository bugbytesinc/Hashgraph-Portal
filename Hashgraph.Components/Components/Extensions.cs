using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Hashgraph.Components;

internal static class Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void OpenElement(this RenderTreeBuilder builder, string elementName, [CallerLineNumber] int lineNo = 0)
    {
        builder.OpenElement(lineNo, elementName);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddAttribute(this RenderTreeBuilder builder, string name, [CallerLineNumber] int lineNo = 0)
    {
        builder.AddAttribute(lineNo, name);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddMultipleAttributes(this RenderTreeBuilder builder, IEnumerable<KeyValuePair<string, object>>? attributes, [CallerLineNumber] int lineNo = 0)
    {
        builder.AddMultipleAttributes(lineNo, attributes);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddAttribute(this RenderTreeBuilder builder, string name, string? value, [CallerLineNumber] int lineNo = 0)
    {
        builder.AddAttribute(lineNo, name, value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddAttribute<TArgument>(this RenderTreeBuilder builder, string name, EventCallback<TArgument> value, [CallerLineNumber] int lineNo = 0)
    {
        builder.AddAttribute<TArgument>(lineNo, name, value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void OpenRegion(this RenderTreeBuilder builder, [CallerLineNumber] int lineNo = 0)
    {
        builder.OpenRegion(lineNo);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddContent(this RenderTreeBuilder builder, string? textContent, [CallerLineNumber] int lineNo = 0)
    {
        builder.AddContent(lineNo, textContent);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddContent(this RenderTreeBuilder builder, object? textContent, [CallerLineNumber] int lineNo = 0)
    {
        builder.AddContent(lineNo, textContent);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddMarkupContent(this RenderTreeBuilder builder, string? markupContent, [CallerLineNumber] int lineNo = 0)
    {
        builder.AddMarkupContent(lineNo, markupContent);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddAttribute(this RenderTreeBuilder builder, string name, object? value, [CallerLineNumber] int lineNo = 0)
    {
        builder.AddAttribute(lineNo, name, value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void OpenComponent<TComponent>(this RenderTreeBuilder builder, [CallerLineNumber] int lineNo = 0) where TComponent : IComponent
    {
        builder.OpenComponent<TComponent>(lineNo);        
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddComponentReferenceCapture(this RenderTreeBuilder builder, Action<object> componentReferenceCaptureAction, [CallerLineNumber] int lineNo = 0)
    {
        builder.AddComponentReferenceCapture(lineNo, componentReferenceCaptureAction);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddElementReferenceCapture(this RenderTreeBuilder builder, Action<ElementReference> elementReferenceCaptureAction, [CallerLineNumber] int lineNo = 0)
    {
        builder.AddElementReferenceCapture(lineNo, elementReferenceCaptureAction);
    }
    internal static bool IsNullOrNone([NotNullWhen(false)] this Address? address)
    {
        if (address is null)
        {
            return true;
        }
        if (Address.None.Equals(address))
        {
            return true;
        }
        return false;
    }
    internal static bool IsNullOrNone([NotNullWhen(false)] this Endorsement? endorsement)
    {
        if (endorsement is null)
        {
            return true;
        }
        if (Address.None.Equals(endorsement))
        {
            return true;
        }
        return false;
    }
}
