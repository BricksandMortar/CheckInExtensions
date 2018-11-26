<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RetrospectiveAttendanceInput.ascx.cs" Inherits="Plugins.com_bricksandmortarstudio.CheckInExtensions.RetrospectiveAttendanceInput" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square-o"></i>Attendance Input</h1>
            </div>
            <div class="panel-body" runat="server">
                <Rock:NotificationBox ID="nbDisplayBox" runat="server" />
                <asp:HiddenField ID="hfIsDirty" runat="server" Value="false" />
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
                    <div class="col-md-3" id="personAdd">
                        <Rock:PersonPicker Label="Person" ID="ppAttendee" CssClass="personAdd" EnableSelfSelection="False" IncludeBusinesses="False" OnSelectPerson="ppAttendee_OnSelectPerson" runat="server" />
                    </div>
                    <div class="col-md-4">
                        <asp:LinkButton ID="btnSave" OnClick="Save" AccessKey="s" CssClass="btn btn-primary" runat="server" CausesValidation="False">Save</asp:LinkButton>
                    </div>
                    <div class="col-md-4">
                    </div>
                </div>
                <div class="grid grid-panel" style="margin-top: 1em">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="False" DataKeyNames="Guid">
                        <Columns>
                            <Rock:RockBoundField DataField="PersonAlias.Person.FullName" HeaderText="Name" />
                            <Rock:DeleteField OnClick="GListRemove" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>