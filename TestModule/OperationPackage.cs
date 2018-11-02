// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: OperationPackage.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace ReportService {

  /// <summary>Holder for reflection information generated from OperationPackage.proto</summary>
  public static partial class OperationPackageReflection {

    #region Descriptor
    /// <summary>File descriptor for OperationPackage.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static OperationPackageReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChZPcGVyYXRpb25QYWNrYWdlLnByb3RvEg1SZXBvcnRTZXJ2aWNlImQKEE9w",
            "ZXJhdGlvblBhY2thZ2USDwoHQ3JlYXRlZBgBIAEoBRIVCg1PcGVyYXRpb25O",
            "YW1lGAIgASgJEigKCERhdGFTZXRzGA8gAygLMhYuUmVwb3J0U2VydmljZS5E",
            "YXRhU2V0ImUKB0RhdGFTZXQSDAoETmFtZRgBIAEoCRIqCgdDb2x1bW5zGAIg",
            "AygLMhkuUmVwb3J0U2VydmljZS5Db2x1bW5JbmZvEiAKBFJvd3MYAyADKAsy",
            "Ei5SZXBvcnRTZXJ2aWNlLlJvdyJVCgpDb2x1bW5JbmZvEgwKBE5hbWUYASAB",
            "KAkSJwoEVHlwZRgCIAEoDjIZLlJlcG9ydFNlcnZpY2UuU2NhbGFyVHlwZRIQ",
            "CghOdWxsYWJsZRgDIAEoCCIyCgNSb3cSKwoGVmFsdWVzGAEgAygLMhsuUmVw",
            "b3J0U2VydmljZS5WYXJpYW50VmFsdWUilgEKDFZhcmlhbnRWYWx1ZRISCgpJ",
            "bnQzMlZhbHVlGAEgASgFEhMKC0RvdWJsZVZhbHVlGAIgASgBEhEKCUxvbmdW",
            "YWx1ZRgDIAEoAxIRCglCb29sVmFsdWUYBCABKAgSEwoLU3RyaW5nVmFsdWUY",
            "BSABKAkSEgoKQnl0ZXNWYWx1ZRgGIAEoDBIOCgZJc051bGwYDyABKAgqTgoK",
            "U2NhbGFyVHlwZRIJCgVJbnQzMhAAEgoKBkRvdWJsZRABEggKBExvbmcQAhII",
            "CgRCb29sEAMSCgoGU3RyaW5nEAQSCQoFQnl0ZXMQBWIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::ReportService.ScalarType), }, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::ReportService.OperationPackage), global::ReportService.OperationPackage.Parser, new[]{ "Created", "OperationName", "DataSets" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::ReportService.DataSet), global::ReportService.DataSet.Parser, new[]{ "Name", "Columns", "Rows" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::ReportService.ColumnInfo), global::ReportService.ColumnInfo.Parser, new[]{ "Name", "Type", "Nullable" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::ReportService.Row), global::ReportService.Row.Parser, new[]{ "Values" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::ReportService.VariantValue), global::ReportService.VariantValue.Parser, new[]{ "Int32Value", "DoubleValue", "LongValue", "BoolValue", "StringValue", "BytesValue", "IsNull" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Enums
  public enum ScalarType {
    [pbr::OriginalName("Int32")] Int32 = 0,
    [pbr::OriginalName("Double")] Double = 1,
    [pbr::OriginalName("Long")] Long = 2,
    [pbr::OriginalName("Bool")] Bool = 3,
    [pbr::OriginalName("String")] String = 4,
    /// <summary>
    ///Can add other if needed. Now no need i think
    /// </summary>
    [pbr::OriginalName("Bytes")] Bytes = 5,
  }

  #endregion

  #region Messages
  public sealed partial class OperationPackage : pb::IMessage<OperationPackage> {
    private static readonly pb::MessageParser<OperationPackage> _parser = new pb::MessageParser<OperationPackage>(() => new OperationPackage());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<OperationPackage> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::ReportService.OperationPackageReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public OperationPackage() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public OperationPackage(OperationPackage other) : this() {
      created_ = other.created_;
      operationName_ = other.operationName_;
      dataSets_ = other.dataSets_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public OperationPackage Clone() {
      return new OperationPackage(this);
    }

    /// <summary>Field number for the "Created" field.</summary>
    public const int CreatedFieldNumber = 1;
    private int created_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Created {
      get { return created_; }
      set {
        created_ = value;
      }
    }

    /// <summary>Field number for the "OperationName" field.</summary>
    public const int OperationNameFieldNumber = 2;
    private string operationName_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string OperationName {
      get { return operationName_; }
      set {
        operationName_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "DataSets" field.</summary>
    public const int DataSetsFieldNumber = 15;
    private static readonly pb::FieldCodec<global::ReportService.DataSet> _repeated_dataSets_codec
        = pb::FieldCodec.ForMessage(122, global::ReportService.DataSet.Parser);
    private readonly pbc::RepeatedField<global::ReportService.DataSet> dataSets_ = new pbc::RepeatedField<global::ReportService.DataSet>();
    /// <summary>
    ///if need to add any no-repeated fields,can place them at 3-14
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::ReportService.DataSet> DataSets {
      get { return dataSets_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as OperationPackage);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(OperationPackage other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Created != other.Created) return false;
      if (OperationName != other.OperationName) return false;
      if(!dataSets_.Equals(other.dataSets_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Created != 0) hash ^= Created.GetHashCode();
      if (OperationName.Length != 0) hash ^= OperationName.GetHashCode();
      hash ^= dataSets_.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Created != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(Created);
      }
      if (OperationName.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(OperationName);
      }
      dataSets_.WriteTo(output, _repeated_dataSets_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Created != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Created);
      }
      if (OperationName.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(OperationName);
      }
      size += dataSets_.CalculateSize(_repeated_dataSets_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(OperationPackage other) {
      if (other == null) {
        return;
      }
      if (other.Created != 0) {
        Created = other.Created;
      }
      if (other.OperationName.Length != 0) {
        OperationName = other.OperationName;
      }
      dataSets_.Add(other.dataSets_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            Created = input.ReadInt32();
            break;
          }
          case 18: {
            OperationName = input.ReadString();
            break;
          }
          case 122: {
            dataSets_.AddEntriesFrom(input, _repeated_dataSets_codec);
            break;
          }
        }
      }
    }

  }

  public sealed partial class DataSet : pb::IMessage<DataSet> {
    private static readonly pb::MessageParser<DataSet> _parser = new pb::MessageParser<DataSet>(() => new DataSet());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<DataSet> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::ReportService.OperationPackageReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public DataSet() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public DataSet(DataSet other) : this() {
      name_ = other.name_;
      columns_ = other.columns_.Clone();
      rows_ = other.rows_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public DataSet Clone() {
      return new DataSet(this);
    }

    /// <summary>Field number for the "Name" field.</summary>
    public const int NameFieldNumber = 1;
    private string name_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Name {
      get { return name_; }
      set {
        name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "Columns" field.</summary>
    public const int ColumnsFieldNumber = 2;
    private static readonly pb::FieldCodec<global::ReportService.ColumnInfo> _repeated_columns_codec
        = pb::FieldCodec.ForMessage(18, global::ReportService.ColumnInfo.Parser);
    private readonly pbc::RepeatedField<global::ReportService.ColumnInfo> columns_ = new pbc::RepeatedField<global::ReportService.ColumnInfo>();
    /// <summary>
    ///maybe 14-15 tags instead of 2-3?
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::ReportService.ColumnInfo> Columns {
      get { return columns_; }
    }

    /// <summary>Field number for the "Rows" field.</summary>
    public const int RowsFieldNumber = 3;
    private static readonly pb::FieldCodec<global::ReportService.Row> _repeated_rows_codec
        = pb::FieldCodec.ForMessage(26, global::ReportService.Row.Parser);
    private readonly pbc::RepeatedField<global::ReportService.Row> rows_ = new pbc::RepeatedField<global::ReportService.Row>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::ReportService.Row> Rows {
      get { return rows_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as DataSet);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(DataSet other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Name != other.Name) return false;
      if(!columns_.Equals(other.columns_)) return false;
      if(!rows_.Equals(other.rows_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Name.Length != 0) hash ^= Name.GetHashCode();
      hash ^= columns_.GetHashCode();
      hash ^= rows_.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Name.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Name);
      }
      columns_.WriteTo(output, _repeated_columns_codec);
      rows_.WriteTo(output, _repeated_rows_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Name.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      size += columns_.CalculateSize(_repeated_columns_codec);
      size += rows_.CalculateSize(_repeated_rows_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(DataSet other) {
      if (other == null) {
        return;
      }
      if (other.Name.Length != 0) {
        Name = other.Name;
      }
      columns_.Add(other.columns_);
      rows_.Add(other.rows_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            Name = input.ReadString();
            break;
          }
          case 18: {
            columns_.AddEntriesFrom(input, _repeated_columns_codec);
            break;
          }
          case 26: {
            rows_.AddEntriesFrom(input, _repeated_rows_codec);
            break;
          }
        }
      }
    }

  }

  public sealed partial class ColumnInfo : pb::IMessage<ColumnInfo> {
    private static readonly pb::MessageParser<ColumnInfo> _parser = new pb::MessageParser<ColumnInfo>(() => new ColumnInfo());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<ColumnInfo> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::ReportService.OperationPackageReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ColumnInfo() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ColumnInfo(ColumnInfo other) : this() {
      name_ = other.name_;
      type_ = other.type_;
      nullable_ = other.nullable_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ColumnInfo Clone() {
      return new ColumnInfo(this);
    }

    /// <summary>Field number for the "Name" field.</summary>
    public const int NameFieldNumber = 1;
    private string name_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Name {
      get { return name_; }
      set {
        name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "Type" field.</summary>
    public const int TypeFieldNumber = 2;
    private global::ReportService.ScalarType type_ = 0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::ReportService.ScalarType Type {
      get { return type_; }
      set {
        type_ = value;
      }
    }

    /// <summary>Field number for the "Nullable" field.</summary>
    public const int NullableFieldNumber = 3;
    private bool nullable_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Nullable {
      get { return nullable_; }
      set {
        nullable_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as ColumnInfo);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(ColumnInfo other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Name != other.Name) return false;
      if (Type != other.Type) return false;
      if (Nullable != other.Nullable) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Name.Length != 0) hash ^= Name.GetHashCode();
      if (Type != 0) hash ^= Type.GetHashCode();
      if (Nullable != false) hash ^= Nullable.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Name.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Name);
      }
      if (Type != 0) {
        output.WriteRawTag(16);
        output.WriteEnum((int) Type);
      }
      if (Nullable != false) {
        output.WriteRawTag(24);
        output.WriteBool(Nullable);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Name.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      if (Type != 0) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Type);
      }
      if (Nullable != false) {
        size += 1 + 1;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(ColumnInfo other) {
      if (other == null) {
        return;
      }
      if (other.Name.Length != 0) {
        Name = other.Name;
      }
      if (other.Type != 0) {
        Type = other.Type;
      }
      if (other.Nullable != false) {
        Nullable = other.Nullable;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            Name = input.ReadString();
            break;
          }
          case 16: {
            type_ = (global::ReportService.ScalarType) input.ReadEnum();
            break;
          }
          case 24: {
            Nullable = input.ReadBool();
            break;
          }
        }
      }
    }

  }

  public sealed partial class Row : pb::IMessage<Row> {
    private static readonly pb::MessageParser<Row> _parser = new pb::MessageParser<Row>(() => new Row());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Row> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::ReportService.OperationPackageReflection.Descriptor.MessageTypes[3]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Row() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Row(Row other) : this() {
      values_ = other.values_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Row Clone() {
      return new Row(this);
    }

    /// <summary>Field number for the "Values" field.</summary>
    public const int ValuesFieldNumber = 1;
    private static readonly pb::FieldCodec<global::ReportService.VariantValue> _repeated_values_codec
        = pb::FieldCodec.ForMessage(10, global::ReportService.VariantValue.Parser);
    private readonly pbc::RepeatedField<global::ReportService.VariantValue> values_ = new pbc::RepeatedField<global::ReportService.VariantValue>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::ReportService.VariantValue> Values {
      get { return values_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Row);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Row other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!values_.Equals(other.values_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= values_.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      values_.WriteTo(output, _repeated_values_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      size += values_.CalculateSize(_repeated_values_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Row other) {
      if (other == null) {
        return;
      }
      values_.Add(other.values_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            values_.AddEntriesFrom(input, _repeated_values_codec);
            break;
          }
        }
      }
    }

  }

  public sealed partial class VariantValue : pb::IMessage<VariantValue> {
    private static readonly pb::MessageParser<VariantValue> _parser = new pb::MessageParser<VariantValue>(() => new VariantValue());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<VariantValue> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::ReportService.OperationPackageReflection.Descriptor.MessageTypes[4]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public VariantValue() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public VariantValue(VariantValue other) : this() {
      int32Value_ = other.int32Value_;
      doubleValue_ = other.doubleValue_;
      longValue_ = other.longValue_;
      boolValue_ = other.boolValue_;
      stringValue_ = other.stringValue_;
      bytesValue_ = other.bytesValue_;
      isNull_ = other.isNull_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public VariantValue Clone() {
      return new VariantValue(this);
    }

    /// <summary>Field number for the "Int32Value" field.</summary>
    public const int Int32ValueFieldNumber = 1;
    private int int32Value_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Int32Value {
      get { return int32Value_; }
      set {
        int32Value_ = value;
      }
    }

    /// <summary>Field number for the "DoubleValue" field.</summary>
    public const int DoubleValueFieldNumber = 2;
    private double doubleValue_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public double DoubleValue {
      get { return doubleValue_; }
      set {
        doubleValue_ = value;
      }
    }

    /// <summary>Field number for the "LongValue" field.</summary>
    public const int LongValueFieldNumber = 3;
    private long longValue_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public long LongValue {
      get { return longValue_; }
      set {
        longValue_ = value;
      }
    }

    /// <summary>Field number for the "BoolValue" field.</summary>
    public const int BoolValueFieldNumber = 4;
    private bool boolValue_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool BoolValue {
      get { return boolValue_; }
      set {
        boolValue_ = value;
      }
    }

    /// <summary>Field number for the "StringValue" field.</summary>
    public const int StringValueFieldNumber = 5;
    private string stringValue_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string StringValue {
      get { return stringValue_; }
      set {
        stringValue_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "BytesValue" field.</summary>
    public const int BytesValueFieldNumber = 6;
    private pb::ByteString bytesValue_ = pb::ByteString.Empty;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pb::ByteString BytesValue {
      get { return bytesValue_; }
      set {
        bytesValue_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "IsNull" field.</summary>
    public const int IsNullFieldNumber = 15;
    private bool isNull_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool IsNull {
      get { return isNull_; }
      set {
        isNull_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as VariantValue);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(VariantValue other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Int32Value != other.Int32Value) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.Equals(DoubleValue, other.DoubleValue)) return false;
      if (LongValue != other.LongValue) return false;
      if (BoolValue != other.BoolValue) return false;
      if (StringValue != other.StringValue) return false;
      if (BytesValue != other.BytesValue) return false;
      if (IsNull != other.IsNull) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Int32Value != 0) hash ^= Int32Value.GetHashCode();
      if (DoubleValue != 0D) hash ^= pbc::ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.GetHashCode(DoubleValue);
      if (LongValue != 0L) hash ^= LongValue.GetHashCode();
      if (BoolValue != false) hash ^= BoolValue.GetHashCode();
      if (StringValue.Length != 0) hash ^= StringValue.GetHashCode();
      if (BytesValue.Length != 0) hash ^= BytesValue.GetHashCode();
      if (IsNull != false) hash ^= IsNull.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Int32Value != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(Int32Value);
      }
      if (DoubleValue != 0D) {
        output.WriteRawTag(17);
        output.WriteDouble(DoubleValue);
      }
      if (LongValue != 0L) {
        output.WriteRawTag(24);
        output.WriteInt64(LongValue);
      }
      if (BoolValue != false) {
        output.WriteRawTag(32);
        output.WriteBool(BoolValue);
      }
      if (StringValue.Length != 0) {
        output.WriteRawTag(42);
        output.WriteString(StringValue);
      }
      if (BytesValue.Length != 0) {
        output.WriteRawTag(50);
        output.WriteBytes(BytesValue);
      }
      if (IsNull != false) {
        output.WriteRawTag(120);
        output.WriteBool(IsNull);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Int32Value != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Int32Value);
      }
      if (DoubleValue != 0D) {
        size += 1 + 8;
      }
      if (LongValue != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(LongValue);
      }
      if (BoolValue != false) {
        size += 1 + 1;
      }
      if (StringValue.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(StringValue);
      }
      if (BytesValue.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeBytesSize(BytesValue);
      }
      if (IsNull != false) {
        size += 1 + 1;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(VariantValue other) {
      if (other == null) {
        return;
      }
      if (other.Int32Value != 0) {
        Int32Value = other.Int32Value;
      }
      if (other.DoubleValue != 0D) {
        DoubleValue = other.DoubleValue;
      }
      if (other.LongValue != 0L) {
        LongValue = other.LongValue;
      }
      if (other.BoolValue != false) {
        BoolValue = other.BoolValue;
      }
      if (other.StringValue.Length != 0) {
        StringValue = other.StringValue;
      }
      if (other.BytesValue.Length != 0) {
        BytesValue = other.BytesValue;
      }
      if (other.IsNull != false) {
        IsNull = other.IsNull;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            Int32Value = input.ReadInt32();
            break;
          }
          case 17: {
            DoubleValue = input.ReadDouble();
            break;
          }
          case 24: {
            LongValue = input.ReadInt64();
            break;
          }
          case 32: {
            BoolValue = input.ReadBool();
            break;
          }
          case 42: {
            StringValue = input.ReadString();
            break;
          }
          case 50: {
            BytesValue = input.ReadBytes();
            break;
          }
          case 120: {
            IsNull = input.ReadBool();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
