// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: ExampleProto.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace ReportService {

  /// <summary>Holder for reflection information generated from ExampleProto.proto</summary>
  public static partial class ExampleProtoReflection {

    #region Descriptor
    /// <summary>File descriptor for ExampleProto.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static ExampleProtoReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChJFeGFtcGxlUHJvdG8ucHJvdG8SDVJlcG9ydFNlcnZpY2UiVgoTVGVzdGlu",
            "Z1Byb3RvTWVzc2FnZRIqCgdDb2x1bW5zGAEgAygLMhkuUmVwb3J0U2Vydmlj",
            "ZS5Db2x1bW5JbmZvEhMKC0NvbHVtbkNvdW50GAIgASgFIj0KCkNvbHVtbklu",
            "Zm8SDwoDVGFnGAEgAygFQgIQARIMCgROYW1lGAIgAygJEhAKCFR5cGVOYW1l",
            "GAMgASgJYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::ReportService.TestingProtoMessage), global::ReportService.TestingProtoMessage.Parser, new[]{ "Columns", "ColumnCount" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::ReportService.ColumnInfo), global::ReportService.ColumnInfo.Parser, new[]{ "Tag", "Name", "TypeName" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class TestingProtoMessage : pb::IMessage<TestingProtoMessage> {
    private static readonly pb::MessageParser<TestingProtoMessage> _parser = new pb::MessageParser<TestingProtoMessage>(() => new TestingProtoMessage());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<TestingProtoMessage> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::ReportService.ExampleProtoReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TestingProtoMessage() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TestingProtoMessage(TestingProtoMessage other) : this() {
      columns_ = other.columns_.Clone();
      columnCount_ = other.columnCount_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TestingProtoMessage Clone() {
      return new TestingProtoMessage(this);
    }

    /// <summary>Field number for the "Columns" field.</summary>
    public const int ColumnsFieldNumber = 1;
    private static readonly pb::FieldCodec<global::ReportService.ColumnInfo> _repeated_columns_codec
        = pb::FieldCodec.ForMessage(10, global::ReportService.ColumnInfo.Parser);
    private readonly pbc::RepeatedField<global::ReportService.ColumnInfo> columns_ = new pbc::RepeatedField<global::ReportService.ColumnInfo>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::ReportService.ColumnInfo> Columns {
      get { return columns_; }
    }

    /// <summary>Field number for the "ColumnCount" field.</summary>
    public const int ColumnCountFieldNumber = 2;
    private int columnCount_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int ColumnCount {
      get { return columnCount_; }
      set {
        columnCount_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as TestingProtoMessage);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(TestingProtoMessage other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!columns_.Equals(other.columns_)) return false;
      if (ColumnCount != other.ColumnCount) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= columns_.GetHashCode();
      if (ColumnCount != 0) hash ^= ColumnCount.GetHashCode();
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
      columns_.WriteTo(output, _repeated_columns_codec);
      if (ColumnCount != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(ColumnCount);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      size += columns_.CalculateSize(_repeated_columns_codec);
      if (ColumnCount != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(ColumnCount);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(TestingProtoMessage other) {
      if (other == null) {
        return;
      }
      columns_.Add(other.columns_);
      if (other.ColumnCount != 0) {
        ColumnCount = other.ColumnCount;
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
            columns_.AddEntriesFrom(input, _repeated_columns_codec);
            break;
          }
          case 16: {
            ColumnCount = input.ReadInt32();
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
      get { return global::ReportService.ExampleProtoReflection.Descriptor.MessageTypes[1]; }
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
      tag_ = other.tag_.Clone();
      name_ = other.name_.Clone();
      typeName_ = other.typeName_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ColumnInfo Clone() {
      return new ColumnInfo(this);
    }

    /// <summary>Field number for the "Tag" field.</summary>
    public const int TagFieldNumber = 1;
    private static readonly pb::FieldCodec<int> _repeated_tag_codec
        = pb::FieldCodec.ForInt32(10);
    private readonly pbc::RepeatedField<int> tag_ = new pbc::RepeatedField<int>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<int> Tag {
      get { return tag_; }
    }

    /// <summary>Field number for the "Name" field.</summary>
    public const int NameFieldNumber = 2;
    private static readonly pb::FieldCodec<string> _repeated_name_codec
        = pb::FieldCodec.ForString(18);
    private readonly pbc::RepeatedField<string> name_ = new pbc::RepeatedField<string>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<string> Name {
      get { return name_; }
    }

    /// <summary>Field number for the "TypeName" field.</summary>
    public const int TypeNameFieldNumber = 3;
    private string typeName_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string TypeName {
      get { return typeName_; }
      set {
        typeName_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
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
      if(!tag_.Equals(other.tag_)) return false;
      if(!name_.Equals(other.name_)) return false;
      if (TypeName != other.TypeName) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= tag_.GetHashCode();
      hash ^= name_.GetHashCode();
      if (TypeName.Length != 0) hash ^= TypeName.GetHashCode();
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
      tag_.WriteTo(output, _repeated_tag_codec);
      name_.WriteTo(output, _repeated_name_codec);
      if (TypeName.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(TypeName);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      size += tag_.CalculateSize(_repeated_tag_codec);
      size += name_.CalculateSize(_repeated_name_codec);
      if (TypeName.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(TypeName);
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
      tag_.Add(other.tag_);
      name_.Add(other.name_);
      if (other.TypeName.Length != 0) {
        TypeName = other.TypeName;
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
          case 10:
          case 8: {
            tag_.AddEntriesFrom(input, _repeated_tag_codec);
            break;
          }
          case 18: {
            name_.AddEntriesFrom(input, _repeated_name_codec);
            break;
          }
          case 26: {
            TypeName = input.ReadString();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
