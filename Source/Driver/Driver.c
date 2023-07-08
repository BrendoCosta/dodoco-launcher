#include "Driver.h"

DRIVER_INITIALIZE DriverEntry;
DRIVER_UNLOAD DriverUnload;
DRIVER_DISPATCH DriverCreateClose;
DRIVER_DISPATCH DriverControl;
HANDLE logFileHandle = NULL;
NTSTATUS logFileStatus;

NTSTATUS WriteToDriverLog(LPCWSTR message) {

    if (!logFileHandle)
        return -1;

    IO_STATUS_BLOCK writeIosb;
    RtlZeroMemory(&writeIosb, sizeof(IO_STATUS_BLOCK));

    LONG actualBytes = 0;
    ULONG poolTag = (ULONG) 202307061315;
    const int utf8StringBufferSize = 8189 + 3;
    PCHAR utf8StringBuffer = (CHAR*) ExAllocatePoolWithTag(NonPagedPool, utf8StringBufferSize, poolTag);
    RtlUnicodeToUTF8N(utf8StringBuffer, utf8StringBufferSize, &actualBytes, message, wcslen(message) * sizeof(WCHAR));
    strcat(utf8StringBuffer, "\r\n");

    NTSTATUS status = ZwWriteFile(
        logFileHandle,
        NULL,
        NULL,
        NULL,
        &writeIosb,
        (PVOID) utf8StringBuffer,
        actualBytes + 2,
        NULL,
        NULL
    );

    ExFreePoolWithTag(utf8StringBuffer, poolTag);
    return status;

}

NTSTATUS WriteBufferToDriverLog(PVOID buffer, size_t bufferLength) {

    if (!logFileHandle)
        return -1;

    IO_STATUS_BLOCK writeIosb;
    RtlZeroMemory(&writeIosb, sizeof(IO_STATUS_BLOCK));

    return ZwWriteFile(
        logFileHandle,
        NULL,
        NULL,
        NULL,
        &writeIosb,
        buffer,
        bufferLength,
        NULL,
        NULL
    );

}

NTSTATUS DriverEntry(PDRIVER_OBJECT DriverObject, PUNICODE_STRING RegistryPath) {
    
    UNICODE_STRING logFileName = RTL_CONSTANT_STRING(L"\\??\\C:\\dodocoDriverHookLog");
    OBJECT_ATTRIBUTES logFileAttributes;
    IO_STATUS_BLOCK logFileIosb;
    RtlZeroMemory(&logFileIosb, sizeof(IO_STATUS_BLOCK));
    InitializeObjectAttributes(&logFileAttributes, &logFileName, OBJ_CASE_INSENSITIVE, NULL, NULL);

    logFileStatus = ZwCreateFile(
        &logFileHandle,
        FILE_APPEND_DATA | SYNCHRONIZE,
        &logFileAttributes,
        &logFileIosb,
        0,
        FILE_ATTRIBUTE_NORMAL,
        0,
        FILE_SUPERSEDE,
        FILE_SYNCHRONOUS_IO_NONALERT,
        NULL,
        0
    );

    WriteToDriverLog(L"Setting up driver object...");

    DriverObject->MajorFunction[IRP_MJ_CREATE] = DriverCreateClose;
	DriverObject->MajorFunction[IRP_MJ_CLOSE] = DriverCreateClose;
	DriverObject->MajorFunction[IRP_MJ_DEVICE_CONTROL] = DriverControl;
    DriverObject->DriverUnload = DriverUnload;

    WriteToDriverLog(L"Creating driver device...");

    UNICODE_STRING devName = RTL_CONSTANT_STRING(DRIVER_DEVICE_PATH);
	PDEVICE_OBJECT DeviceObject;
	NTSTATUS status = IoCreateDevice(DriverObject, 0, &devName, FILE_DEVICE_UNKNOWN, 0, FALSE, &DeviceObject);

    if (!NT_SUCCESS(status)) {
        
        WriteToDriverLog(L"IoCreateDevice error");
        return status;

    }

    WriteToDriverLog(L"Successfully created driver device");
    WriteToDriverLog(DRIVER_DEVICE_PATH);
    WriteToDriverLog(L"Creating driver device symbolic link...");

	UNICODE_STRING symLink = RTL_CONSTANT_STRING(DRIVER_SYMLINK_PATH);
	status = IoCreateSymbolicLink(&symLink, &devName);
	
    if (!NT_SUCCESS(status)) {

        WriteToDriverLog(L"IoCreateSymbolicLink error");
        IoDeleteDevice(DeviceObject);
		return status;

	}

    WriteToDriverLog(L"Successfully created driver device symbolic link");
    WriteToDriverLog(DRIVER_SYMLINK_PATH);

    UNREFERENCED_PARAMETER(DriverObject);
	UNREFERENCED_PARAMETER(RegistryPath);

    WriteToDriverLog(L"Successfully initialized the driver");

    return STATUS_SUCCESS;

}

VOID DriverUnload(PDRIVER_OBJECT DriverObject) {

    WriteToDriverLog(L"Successfully unloaded the driver");
    if (logFileHandle)
        ZwClose(logFileHandle);
    return;

}

NTSTATUS DriverCreateClose(PDEVICE_OBJECT DeviceObject, PIRP Irp) {

	UNREFERENCED_PARAMETER(DeviceObject);

	Irp->IoStatus.Status = STATUS_SUCCESS;
	Irp->IoStatus.Information = 0;
	IoCompleteRequest(Irp, IO_NO_INCREMENT);
	return STATUS_SUCCESS;

}

NTSTATUS DriverControl(PDEVICE_OBJECT DeviceObject, PIRP Irp) {

    UNREFERENCED_PARAMETER(DeviceObject);

    NTSTATUS status = STATUS_SUCCESS;
    PIO_STACK_LOCATION stack = IoGetCurrentIrpStackLocation(Irp); 
    
    switch (stack->Parameters.DeviceIoControl.IoControlCode) {

        default:
		    status = STATUS_INVALID_DEVICE_REQUEST;
		    break;

    }

    Irp->IoStatus.Status = status;
    Irp->IoStatus.Information = 0;
    IoCompleteRequest(Irp, IO_NO_INCREMENT);
    return status;

}
