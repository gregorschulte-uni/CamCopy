using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CamCopy
{
    [ComImport, Guid("4db1ad10-3391-11d2-9a33-00c04fa36145")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IWiaItem
    {
        void GetItemType(
            [Out] out int itemType);

        void AnalyzeItem(
            [In] int lFlags);

        void EnumChildItems(
            [Out, MarshalAs(UnmanagedType.Interface)] out object iEnumWiaItem);

        void DeleteItem(
            [In] int lFlags);

        void CreateChildItem(
            [In] int lFlags,
            [In, MarshalAs(UnmanagedType.BStr)] string strItemName,
            [In, MarshalAs(UnmanagedType.BStr)] string strFullItemName,
            [Out, MarshalAs(UnmanagedType.Interface)] out object iWiaItem);

        void EnumRegisterEventInfo(
            [In] int lFlags,
            [In] ref Guid eventGuid,
            [Out, MarshalAs(UnmanagedType.Interface)] out object iEnum);

        void FindItemByName(
            [In] int lFlags,
            [In, MarshalAs(UnmanagedType.BStr)] string strFullItemName,
            [Out, MarshalAs(UnmanagedType.Interface)] out object iWiaItem);

        void DeviceDlg(
            [In] IntPtr hwndParent,
            [In] int lFlags,
            [In] int lIntent,
            [Out] out int iItemCount,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 128)] IWiaItem[] iWiaItem);

        void DeviceCommand(
            [In] int lFlags,
            [In] ref Guid cmdGuid,
            [In, Out, MarshalAs(UnmanagedType.Interface)] ref object iWiaItem);

        void GetRootItem(
            [Out, MarshalAs(UnmanagedType.Interface)] out object iWiaItem);

        void EnumDeviceCapabilities(
            [In] int lFlags,
            [Out, MarshalAs(UnmanagedType.Interface)] out object iEnumWIA_DEV_CAPS);

        void DumpItemData(
            [Out, MarshalAs(UnmanagedType.BStr)] out string strData);

        void DumpDrvItemData(
            [Out, MarshalAs(UnmanagedType.BStr)] out string strData);

        void DumpTreeItemData(
            [Out, MarshalAs(UnmanagedType.BStr)] out string strData);

        void Diagnostic(
            [In] int nSize,
            [In] byte[] buffer);
    }
}
