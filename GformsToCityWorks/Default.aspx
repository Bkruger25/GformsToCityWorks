<%@ Page Title="Home Page" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.vb" Inherits="GformsToCityWorks._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h2>Sync Data between Gforms and Cityworks</h2>
        <h3>(Please ensure views are created!)</h3>
        
        <div class="row">
            <div class="col-md-3">
                <asp:Label Text="Enter a Gform Table Name: " runat="server"></asp:Label>
            </div>
            <div class="col-md-8">
                <asp:TextBox runat="server" id="txtFormName"></asp:TextBox>
            </div>
        </div>
        
        <asp:Button Text="Sync" runat="server" id="btnSync" class="btn btn-primary" />
    </div>


</asp:Content>
