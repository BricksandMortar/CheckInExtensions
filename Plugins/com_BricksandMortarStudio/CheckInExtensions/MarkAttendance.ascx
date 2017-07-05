<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MarkAttendance.ascx.cs" Inherits="Plugins.com_bricksandmortarstudio.CheckInExtensions.MarkAttendance" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square-o"></i>Attendance Input</h1>
            </div>
            <div class="panel-body" runat="server">
                <Rock:NotificationBox ID="nbDisplayBox" runat="server" />
                <div class="row">
                    <div class="col-md-3" id="campusContainer" runat="server">
                        <Rock:RockDropDownList ID="ddlCampuses" runat="server" Label="Campus"
                            Help="The campus to record attendance for." Required="True" AutoPostBack="True" OnSelectedIndexChanged="ddlCampuses_OnSelectedIndexChanged" />
                    </div>
                    <div class="col-md-3">
                        <Rock:RockDropDownList ID="ddlGroups" runat="server" Required="True" Label="Group" Help="The group to record attendance for." OnSelectedIndexChanged="ddlGroups_OnSelectedIndexChanged" AutoPostBack="True" />
                    </div>
                    <div class="col-md-3">
                        <Rock:RockDropDownList runat="server" ID="ddlLocations" Label="Location" Help="Location to record attendance for." Enabled="False" OnSelectedIndexChanged="ddlLocations_OnSelectedIndexChanged" AutoPostBack="True" />
                    </div>
                    <div class="col-md-3">
                        <Rock:RockDropDownList runat="server" ID="ddlInstances" Label="Instance" Help="The instance to save attendance for." OnSelectedIndexChanged="ddlInstances_OnSelectedIndexChanged" Enabled="False" AutoPostBack="True" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-4">
                        <asp:LinkButton ID="btnSave" OnClick="lbSave_Click" AccessKey="s" CssClass="btn btn-primary" runat="server" CausesValidation="False">Save</asp:LinkButton>
                        <asp:LinkButton ID="btnSelectAll" OnClick="lbSelectAll_Click" AccessKey="a" CssClass="btn btn-default" runat="server" CausesValidation="False">Select All</asp:LinkButton>
                    </div>
                    <div class="col-md-4">
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-12">
                        <asp:ListView ID="lvAttendance" runat="server">
                            <ItemTemplate>
                                <asp:HiddenField ID="hfMember" runat="server" Value='<%# Eval("PersonAliasId") %>' />
                                <Rock:RockCheckBox ID="cbMember" OnCheckedChanged="cbMember_OnCheckedChanged" AutoPostBack="True" runat="server" Checked='<%# Bind("Attended") %>' Text='<%# Eval("FullName")%>' />
                            </ItemTemplate>
                        </asp:ListView>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
