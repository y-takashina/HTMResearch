<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Results.aspx.cs" Inherits="ResultVisualization.Results" %>
<%@ Import Namespace="System.IO" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="UTF-8">
    <title>Document</title>
    <style type="text/css">
        #file { width: 209px; }
    </style>
</head>

<body bgcolor="#fff3b8">

<h1>HTM Temporal Pooling</h1>
<h2>Select dropDownList</h2>
<form runat="server">
    <asp:DropDownList id="dropDownList" runat="server"/>
</form>

<h2>Layer 1</h2>
<table>
    <th>transitions</th>
    <th>probabilities</th>
    <th>distances_mean</th>
    <th>distances_min</th>
    <th>membership</th>
    <tr>
        <td>
            <img src="layer1_transitions.png" width="200">
        </td>
        <td>
            <img src="layer1_probabilities.png" width="200">
        </td>
        <td>
            <img src="layer1_distances_mean.png" width="200">
        </td>
        <td>
            <img src="layer1_distances_min.png" width="200">
        </td>
        <td>
            <img src="layer12_membership.png" height="200">
        </td>
    </tr>
</table>

<h2>Layer 2</h2>
<table>
    <th>transitions</th>
    <th>probabilities</th>
    <th>distances_mean</th>
    <th>distances_min</th>
    <th>membership</th>
    <tr>
        <td></td>
        <td>
            <img src="layer2_probabilities.png" width="200">
        </td>
        <td>
            <img src="layer2_distances_mean.png" width="200">
        </td>
        <td>
            <img src="layer2_distances_min.png" width="200">
        </td>
        <td>
            <img src="layer23_membership.png" height="200">
        </td>
    </tr>
</table>

<h2>Layer 3</h2>
<table>
    <th>transitions</th>
    <th>probabilities</th>
    <th>distances_mean</th>
    <th>distances_min</th>
    <tr>
        <td></td>
        <td>
            <img src="layer3_probabilities.png" width="200">
        </td>
        <td>
            <img src="layer3_distances_mean.png" width="200">
        </td>
        <td>
            <img src="layer3_distances_min.png" width="200">
        </td>
    </tr>
</table>

<h2>Results</h2>
<h3>Series</h3>
<img src="original.png" width="640">
<h3>Error(htm)</h3>
<img src="error.png" width="640">
<h3>Error(frequency)</h3>
<img src="error2.png" width="640">
</body>