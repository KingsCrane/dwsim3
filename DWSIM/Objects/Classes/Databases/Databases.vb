'    Copyright 2008-2014 Daniel Wagner O. de Medeiros, Gregor Reichert
'
'    This file is part of DWSIM.
'
'    DWSIM is free software: you can redistribute it and/or modify
'    it under the terms of the GNU General Public License as published by
'    the Free Software Foundation, either version 3 of the License, or
'    (at your option) any later version.
'
'    DWSIM is distributed in the hope that it will be useful,
'    but WITHOUT ANY WARRANTY; without even the implied warranty of
'    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    GNU General Public License for more details.
'
'    You should have received a copy of the GNU General Public License
'    along with DWSIM.  If not, see <http://www.gnu.org/licenses/>.
'
'    Imports DWSIM.SimulationObjects

Imports FileHelpers
Imports System.Xml
Imports System.IO
Imports DWSIM.DWSIM.SimulationObjects.PropertyPackages.Auxiliary
Imports CProp = CoolProp

Namespace DWSIM.Databases

    <DelimitedRecord(";")> <IgnoreFirst()> <System.Serializable()> _
    Public Class ChemSepNameIDPair

        Implements ICloneable

        Public ID As Integer = -1
        Public ChemSepName As String = ""
        Public DWSIMName As String = ""

        Public Function Clone() As Object Implements System.ICloneable.Clone

            Dim newclass As New ChemSepNameIDPair
            With newclass
                .ID = Me.ID
                .ChemSepName = Me.ChemSepName
                .DWSIMName = Me.DWSIMName
            End With
            Return newclass
        End Function

    End Class

    <System.Serializable()> Public Class ChemSep

        Private _ids As System.Collections.Generic.Dictionary(Of Integer, ChemSepNameIDPair)
        Private xmldoc As XmlDocument

        Public ReadOnly Property IDs() As System.Collections.Generic.Dictionary(Of Integer, ChemSepNameIDPair)
            Get
                Return _ids
            End Get
        End Property

        Sub New()

            _ids = New System.Collections.Generic.Dictionary(Of Integer, ChemSepNameIDPair)

            Dim pathsep As Char = System.IO.Path.DirectorySeparatorChar

            Dim csid As ChemSepNameIDPair
            Dim csidc() As ChemSepNameIDPair
            Dim fh1 As New FileHelperEngine(Of ChemSepNameIDPair)
            With fh1
                csidc = .ReadFile(My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "csid.dat")
            End With

            For Each csid In csidc
                Me.IDs.Add(csid.ID, csid.Clone)
            Next

            csid = Nothing
            csidc = Nothing
            fh1 = Nothing

        End Sub

        Public Function GetCSName(ByVal id As String)

            If Me.IDs.ContainsKey(id) Then
                Return Me.IDs(id).ChemSepName
            Else
                Return id
            End If

        End Function

        Public Function GetDWSIMName(ByVal id As String)

            If Me.IDs.ContainsKey(id) Then
                Return Me.IDs(id).DWSIMName
            Else
                Return id
            End If

        End Function

        Public Sub Load(ByVal filename As String)

            Dim mytxt As String = ""
            mytxt = File.ReadAllText(filename)

            xmldoc = New XmlDocument
            xmldoc.LoadXml(mytxt)

            mytxt = Nothing

        End Sub

        Public Function Transfer(Optional ByVal CompName As String = "") As ClassesBasicasTermodinamica.ConstantProperties()

            Dim cp As ClassesBasicasTermodinamica.ConstantProperties
            Dim cpa As New ArrayList()
            Dim cult As Globalization.CultureInfo = New Globalization.CultureInfo("en-US")
            Dim nf As Globalization.NumberFormatInfo = cult.NumberFormat

            Dim unif As New Global.DWSIM.DWSIM.SimulationObjects.PropertyPackages.Auxiliary.Unifac
            Dim modf As New Global.DWSIM.DWSIM.SimulationObjects.PropertyPackages.Auxiliary.Modfac

            For Each node As XmlNode In xmldoc.ChildNodes(0).ChildNodes
                cp = New ClassesBasicasTermodinamica.ConstantProperties
                With cp
                    .CurrentDB = "ChemSep"
                    .OriginalDB = "ChemSep"
                    .IsHYPO = "0"
                    .IsPF = "0"
                End With
                For Each node2 As XmlNode In node.ChildNodes
                    Select Case node2.Name
                        Case "LibraryIndex"
                            cp.ID = node2.Attributes("value").Value
                        Case "CompoundID"
                            cp.Name = node2.Attributes("value").Value
                        Case "StructureFormula"
                            cp.Formula = node2.Attributes("value").Value
                        Case "CriticalTemperature" 'K
                            cp.Critical_Temperature = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "CriticalPressure" 'Pa
                            cp.Critical_Pressure = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "CriticalVolume" 'm3/kmol
                            cp.Critical_Volume = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "CriticalCompressibility"
                            cp.Critical_Compressibility = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "NormalBoilingPointTemperature" 'K
                            cp.Normal_Boiling_Point = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "NormalMeltingPointTemperature"
                            cp.TemperatureOfFusion = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "MolecularWeight"
                            cp.Molar_Weight = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "AcentricityFactor"
                            cp.Acentric_Factor = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "DipoleMoment" 'coloumb.m
                            cp.Dipole_Moment = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "HeatOfFusionAtMeltingPoint" ' kJ/mol
                            cp.EnthalpyOfFusionAtTf = Double.Parse(node2.Attributes("value").Value, nf) / 1000 / 1000
                        Case "HeatOfFormation" '/1000/MW, kJ/kg
                            cp.IG_Enthalpy_of_Formation_25C = Double.Parse(node2.Attributes("value").Value, nf) / 1000 / cp.Molar_Weight
                        Case "GibbsEnergyOfFormation" '/1000/MW, kJ/kg
                            cp.IG_Gibbs_Energy_of_Formation_25C = Double.Parse(node2.Attributes("value").Value, nf) / 1000 / cp.Molar_Weight
                        Case "AbsEntropy" '/1000/MW, kJ/kg
                            cp.IG_Entropy_of_Formation_25C = Double.Parse(node2.Attributes("value").Value, nf) / 1000 / cp.Molar_Weight
                        Case "RacketParameter"
                            cp.Z_Rackett = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "ChaoSeaderAcentricFactor"
                            cp.Chao_Seader_Acentricity = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "ChaoSeaderSolubilityParameter"
                            cp.Chao_Seader_Solubility_Parameter = Double.Parse(node2.Attributes("value").Value, nf) * 0.238846 / 1000000.0
                        Case "ChaoSeaderLiquidVolume"
                            cp.Chao_Seader_Liquid_Molar_Volume = Double.Parse(node2.Attributes("value").Value, nf) * 1000
                        Case "CAS"
                            cp.CAS_Number = node2.Attributes("value").Value
                        Case "Smiles"
                            cp.SMILES = node2.Attributes("value").Value
                        Case "StructureFormula"
                            cp.ChemicalStructure = node2.Attributes("value").Value
                        Case "UniquacR"
                            cp.UNIQUAC_R = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "UniquacQ"
                            cp.UNIQUAC_Q = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "VaporPressure"
                            '<vp_c name="Vapour pressure"  units="Pa" >
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "eqno"
                                        cp.VaporPressureEquation = node3.Attributes("value").Value
                                    Case "A"
                                        cp.Vapor_Pressure_Constant_A = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "B"
                                        cp.Vapor_Pressure_Constant_B = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "C"
                                        cp.Vapor_Pressure_Constant_C = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "D"
                                        cp.Vapor_Pressure_Constant_D = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "E"
                                        cp.Vapor_Pressure_Constant_E = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmin"
                                        cp.Vapor_Pressure_TMIN = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmax"
                                        cp.Vapor_Pressure_TMAX = Double.Parse(node3.Attributes("value").Value, nf)
                                End Select
                            Next
                        Case "HeatOfVaporization"
                            '<hvpc name="Heat of vaporization"  units="J/kmol" >
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "eqno"
                                        cp.VaporizationEnthalpyEquation = node3.Attributes("value").Value
                                    Case "A"
                                        cp.HVap_A = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "B"
                                        cp.HVap_B = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "C"
                                        cp.HVap_C = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "D"
                                        cp.HVap_D = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "E"
                                        cp.HVap_E = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmin"
                                        cp.HVap_TMIN = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmax"
                                        cp.HVap_TMAX = Double.Parse(node3.Attributes("value").Value, nf)
                                End Select
                            Next
                        Case "IdealGasHeatCapacityCp"
                            '<icpc name="Ideal gas heat capacity"  units="J/kmol/K" >
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "eqno"
                                        cp.IdealgasCpEquation = node3.Attributes("value").Value
                                    Case "A"
                                        cp.Ideal_Gas_Heat_Capacity_Const_A = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "B"
                                        cp.Ideal_Gas_Heat_Capacity_Const_B = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "C"
                                        cp.Ideal_Gas_Heat_Capacity_Const_C = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "D"
                                        cp.Ideal_Gas_Heat_Capacity_Const_D = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "E"
                                        cp.Ideal_Gas_Heat_Capacity_Const_E = Double.Parse(node3.Attributes("value").Value, nf)
                                End Select
                            Next
                        Case "LiquidViscosity"
                            '<lvsc name="Liquid viscosity"  units="Pa.s" >
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "eqno"
                                        cp.LiquidViscosityEquation = node3.Attributes("value").Value
                                    Case "A"
                                        cp.Liquid_Viscosity_Const_A = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "B"
                                        cp.Liquid_Viscosity_Const_B = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "C"
                                        cp.Liquid_Viscosity_Const_C = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "D"
                                        cp.Liquid_Viscosity_Const_D = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "E"
                                        cp.Liquid_Viscosity_Const_E = Double.Parse(node3.Attributes("value").Value, nf)
                                End Select
                            Next
                        Case "LiquidThermalConductivity"
                            '- <LiquidThermalConductivity name="Liquid thermal conductivity" units="W/m/K">
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "eqno"
                                        cp.LiquidThermalConductivityEquation = node3.Attributes("value").Value
                                    Case "A"
                                        cp.Liquid_Thermal_Conductivity_Const_A = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "B"
                                        cp.Liquid_Thermal_Conductivity_Const_B = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "C"
                                        cp.Liquid_Thermal_Conductivity_Const_C = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "D"
                                        cp.Liquid_Thermal_Conductivity_Const_D = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "E"
                                        cp.Liquid_Thermal_Conductivity_Const_E = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmin"
                                        cp.Liquid_Thermal_Conductivity_Tmin = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmax"
                                        cp.Liquid_Thermal_Conductivity_Tmax = Double.Parse(node3.Attributes("value").Value, nf)
                                End Select
                            Next
                        Case "SolidDensity"
                            '<SolidDensity name="Solid density"  units="kmol/m3" >
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "eqno"
                                        cp.SolidDensityEquation = node3.Attributes("value").Value
                                    Case "A"
                                        cp.Solid_Density_Const_A = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "B"
                                        cp.Solid_Density_Const_B = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "C"
                                        cp.Solid_Density_Const_C = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "D"
                                        cp.Solid_Density_Const_D = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "E"
                                        cp.Solid_Density_Const_E = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmin"
                                        cp.Solid_Density_Tmin = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmax"
                                        cp.Solid_Density_Tmax = Double.Parse(node3.Attributes("value").Value, nf)
                                End Select
                            Next
                        Case "LiquidDensity"
                            '- <LiquidDensity name="Liquid density" units="kmol/m3">
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "eqno"
                                        cp.LiquidDensityEquation = node3.Attributes("value").Value
                                    Case "A"
                                        cp.Liquid_Density_Const_A = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "B"
                                        cp.Liquid_Density_Const_B = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "C"
                                        cp.Liquid_Density_Const_C = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "D"
                                        cp.Liquid_Density_Const_D = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "E"
                                        cp.Liquid_Density_Const_E = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmin"
                                        cp.Liquid_Density_Tmin = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmax"
                                        cp.Liquid_Density_Tmax = Double.Parse(node3.Attributes("value").Value, nf)
                                End Select
                            Next
                        Case "LiquidHeatCapacityCp"
                            '- <LiquidHeatCapacityCp name="Liquid heat capacity" units="J/kmol/K">
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "eqno"
                                        cp.LiquidHeatCapacityEquation = node3.Attributes("value").Value
                                    Case "A"
                                        cp.Liquid_Heat_Capacity_Const_A = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "B"
                                        cp.Liquid_Heat_Capacity_Const_B = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "C"
                                        cp.Liquid_Heat_Capacity_Const_C = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "D"
                                        cp.Liquid_Heat_Capacity_Const_D = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "E"
                                        cp.Liquid_Heat_Capacity_Const_E = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmin"
                                        cp.Liquid_Heat_Capacity_Tmin = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmax"
                                        cp.Liquid_Heat_Capacity_Tmax = Double.Parse(node3.Attributes("value").Value, nf)
                                End Select
                            Next
                        Case "SolidHeatCapacityCp"
                            '-<SolidHeatCapacityCp name="Solid heat capacity"  units="J/kmol/K" >
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "eqno"
                                        cp.SolidHeatCapacityEquation = node3.Attributes("value").Value
                                    Case "A"
                                        cp.Solid_Heat_Capacity_Const_A = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "B"
                                        cp.Solid_Heat_Capacity_Const_B = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "C"
                                        cp.Solid_Heat_Capacity_Const_C = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "D"
                                        cp.Solid_Heat_Capacity_Const_D = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "E"
                                        cp.Solid_Heat_Capacity_Const_E = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmin"
                                        cp.Solid_Heat_Capacity_Tmin = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmax"
                                        cp.Solid_Heat_Capacity_Tmax = Double.Parse(node3.Attributes("value").Value, nf)
                                End Select
                            Next
                        Case "VaporViscosity" 'Pa.s
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "eqno"
                                        cp.VaporViscosityEquation = node3.Attributes("value").Value
                                    Case "A"
                                        cp.Vapor_Viscosity_Const_A = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "B"
                                        cp.Vapor_Viscosity_Const_B = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "C"
                                        cp.Vapor_Viscosity_Const_C = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "D"
                                        cp.Vapor_Viscosity_Const_D = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "E"
                                        cp.Vapor_Viscosity_Const_E = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmin"
                                        cp.Vapor_Viscosity_Tmin = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmax"
                                        cp.Vapor_Viscosity_Tmax = Double.Parse(node3.Attributes("value").Value, nf)
                                End Select
                            Next
                        Case "VaporThermalConductivity" 'W/m/K
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "eqno"
                                        cp.VaporThermalConductivityEquation = node3.Attributes("value").Value
                                    Case "A"
                                        cp.Vapor_Thermal_Conductivity_Const_A = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "B"
                                        cp.Vapor_Thermal_Conductivity_Const_B = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "C"
                                        cp.Vapor_Thermal_Conductivity_Const_C = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "D"
                                        cp.Vapor_Thermal_Conductivity_Const_D = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "E"
                                        cp.Vapor_Thermal_Conductivity_Const_E = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmin"
                                        cp.Vapor_Thermal_Conductivity_Tmin = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmax"
                                        cp.Vapor_Thermal_Conductivity_Tmax = Double.Parse(node3.Attributes("value").Value, nf)
                                End Select
                            Next
                        Case "SurfaceTension" 'N/m
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "eqno"
                                        cp.SurfaceTensionEquation = node3.Attributes("value").Value
                                    Case "A"
                                        cp.Surface_Tension_Const_A = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "B"
                                        cp.Surface_Tension_Const_B = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "C"
                                        cp.Surface_Tension_Const_C = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "D"
                                        cp.Surface_Tension_Const_D = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "E"
                                        cp.Surface_Tension_Const_E = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmin"
                                        cp.Surface_Tension_Tmin = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmax"
                                        cp.Surface_Tension_Tmax = Double.Parse(node3.Attributes("value").Value, nf)
                                End Select
                            Next
                        Case "UnifacVLE"
                            If cp.UNIFACGroups.Collection Is Nothing Then cp.UNIFACGroups.Collection = New SortedList
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "group"
                                        If Not cp.UNIFACGroups.Collection.ContainsKey(node3.Attributes("id").Value) Then
                                            cp.UNIFACGroups.Collection.Add(node3.Attributes("id").Value, Integer.Parse(node3.Attributes("value").Value))
                                        End If
                                End Select
                            Next
                        Case "ModifiedUnifac"
                            If cp.MODFACGroups.Collection Is Nothing Then cp.MODFACGroups.Collection = New SortedList
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "group"
                                        If Not cp.MODFACGroups.Collection.ContainsKey(node3.Attributes("id").Value) Then
                                            cp.MODFACGroups.Collection.Add(node3.Attributes("id").Value, Integer.Parse(node3.Attributes("value").Value))
                                        End If
                                End Select
                            Next
                            If cp.NISTMODFACGroups.Collection Is Nothing Then cp.NISTMODFACGroups.Collection = New SortedList
                            For Each sg As String In cp.MODFACGroups.Collection.Keys
                                cp.NISTMODFACGroups.Collection.Add(sg, cp.MODFACGroups.Collection(sg))
                            Next
                    End Select
                Next

                If CompName = "" Then
                    cpa.Add(cp)
                Else
                    If cp.Name = CompName Then
                        cpa.Add(cp)
                        Exit For
                    End If
                End If

            Next

            Return cpa.ToArray(Type.GetType("DWSIM.DWSIM.ClassesBasicasTermodinamica.ConstantProperties"))

        End Function

    End Class

    <System.Serializable()> Public Class DWSIM

        Private xmldoc As XmlDocument

        Sub New()

        End Sub

        Public Sub Load(ByVal filename As String)
            Dim pathsep As Char = Path.DirectorySeparatorChar

            Dim settings As New XmlReaderSettings()
            settings.ConformanceLevel = ConformanceLevel.Fragment
            settings.IgnoreWhitespace = True
            settings.IgnoreComments = True
            settings.CheckCharacters = False
            Dim reader As XmlReader = XmlReader.Create(filename)
            reader.Read()

            xmldoc = New XmlDocument
            xmldoc.Load(reader)

        End Sub

        Public Function Transfer(Optional ByVal CompName As String = "") As ClassesBasicasTermodinamica.ConstantProperties()

            Dim cp As ClassesBasicasTermodinamica.ConstantProperties
            Dim cpa As New ArrayList()
            Dim cult As Globalization.CultureInfo = New Globalization.CultureInfo("en-US")
            Dim nf As Globalization.NumberFormatInfo = cult.NumberFormat

            Dim unif As New SimulationObjects.PropertyPackages.Auxiliary.Unifac
            Dim modf As New SimulationObjects.PropertyPackages.Auxiliary.Modfac

            For Each node As XmlNode In xmldoc.ChildNodes(1)
                cp = New ClassesBasicasTermodinamica.ConstantProperties
                With cp
                    .OriginalDB = "DWSIM"
                    For Each node2 As XmlNode In node.ChildNodes
                        Select Case node2.Name
                            Case "Name"
                                .Name = node2.InnerText
                            Case "CAS_Number"
                                .CAS_Number = node2.InnerText
                            Case "Formula"
                                .Formula = node2.InnerText
                            Case "Molar_Weight"
                                .Molar_Weight = Double.Parse(node2.InnerText, nf)
                            Case "Critical_Temperature"
                                .Critical_Temperature = Double.Parse(node2.InnerText, nf)
                            Case "Critical_Pressure"
                                .Critical_Pressure = Double.Parse(node2.InnerText, nf)
                            Case "Critical_Volume"
                                .Critical_Volume = Double.Parse(node2.InnerText, nf)
                            Case "Critical_Compressibility"
                                .Critical_Compressibility = Double.Parse(node2.InnerText, nf)
                            Case "Acentric_Factor"
                                .Acentric_Factor = Double.Parse(node2.InnerText, nf)
                            Case "Z_Rackett"
                                .Z_Rackett = Double.Parse(node2.InnerText, nf)
                            Case "PR_Volume_Translation_Coefficient"
                                .PR_Volume_Translation_Coefficient = Double.Parse(node2.InnerText, nf)
                            Case "SRK_Volume_Translation_Coefficient"
                                .SRK_Volume_Translation_Coefficient = Double.Parse(node2.InnerText, nf)
                            Case "CS_Acentric_Factor"
                                .Chao_Seader_Acentricity = Double.Parse(node2.InnerText, nf)
                            Case "CS_Solubility_Parameter"
                                .Chao_Seader_Solubility_Parameter = Double.Parse(node2.InnerText, nf)
                            Case "CS_Liquid_Molar_Volume"
                                .Chao_Seader_Liquid_Molar_Volume = Double.Parse(node2.InnerText, nf)
                            Case "IG_Entropy_of_Formation_25C"
                                .IG_Entropy_of_Formation_25C = Double.Parse(node2.InnerText, nf)
                            Case "IG_Enthalpy_of_Formation_25C"
                                .IG_Enthalpy_of_Formation_25C = Double.Parse(node2.InnerText, nf)
                            Case "IG_Gibbs_Energy_of_Formation_25C"
                                .IG_Gibbs_Energy_of_Formation_25C = Double.Parse(node2.InnerText, nf)
                            Case "Dipole_Moment"
                                .Dipole_Moment = Double.Parse(node2.InnerText, nf)
                            Case "DIPPR_Vapor_Pressure_Constant_A"
                                .Vapor_Pressure_Constant_A = Double.Parse(node2.InnerText, nf)
                            Case "DIPPR_Vapor_Pressure_Constant_B"
                                .Vapor_Pressure_Constant_B = Double.Parse(node2.InnerText, nf)
                            Case "DIPPR_Vapor_Pressure_Constant_C"
                                .Vapor_Pressure_Constant_C = Double.Parse(node2.InnerText, nf)
                            Case "DIPPR_Vapor_Pressure_Constant_D"
                                .Vapor_Pressure_Constant_D = Double.Parse(node2.InnerText, nf)
                            Case "DIPPR_Vapor_Pressure_Constant_E"
                                .Vapor_Pressure_Constant_E = Double.Parse(node2.InnerText, nf)
                            Case "DIPPR_Vapor_Pressure_TMIN"
                                .Vapor_Pressure_TMIN = Double.Parse(node2.InnerText, nf)
                            Case "DIPPR_Vapor_Pressure_TMAX"
                                .Vapor_Pressure_TMAX = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Heat_Capacity_Const_A"
                                .Ideal_Gas_Heat_Capacity_Const_A = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Heat_Capacity_Const_B"
                                .Ideal_Gas_Heat_Capacity_Const_B = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Heat_Capacity_Const_C"
                                .Ideal_Gas_Heat_Capacity_Const_C = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Heat_Capacity_Const_D"
                                .Ideal_Gas_Heat_Capacity_Const_D = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Heat_Capacity_Const_E"
                                .Ideal_Gas_Heat_Capacity_Const_E = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Viscosity_Const_A"
                                .Liquid_Viscosity_Const_A = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Viscosity_Const_B"
                                .Liquid_Viscosity_Const_B = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Viscosity_Const_C"
                                .Liquid_Viscosity_Const_C = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Viscosity_Const_D"
                                .Liquid_Viscosity_Const_D = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Viscosity_Const_E"
                                .Liquid_Viscosity_Const_E = Double.Parse(node2.InnerText, nf)
                            Case "Normal_Boiling_Point"
                                .Normal_Boiling_Point = Double.Parse(node2.InnerText, nf)
                            Case "ID"
                                .ID = Integer.Parse(node2.InnerText)
                            Case "isPf"
                                .IsPF = Integer.Parse(node2.InnerText)
                            Case "isHYP"
                                .IsHYPO = Integer.Parse(node2.InnerText)
                            Case "HVapA"
                                .HVap_A = Double.Parse(node2.InnerText, nf)
                            Case "HVapB"
                                .HVap_B = Double.Parse(node2.InnerText, nf)
                            Case "HvapC"
                                .HVap_C = Double.Parse(node2.InnerText, nf)
                            Case "HVapD"
                                .HVap_D = Double.Parse(node2.InnerText, nf)
                            Case "HvapTmin"
                                .HVap_TMIN = Double.Parse(node2.InnerText, nf)
                            Case "HvapTMAX"
                                .HVap_TMAX = Double.Parse(node2.InnerText, nf)
                            Case "UNIQUAC_r"
                                If node2.InnerText <> "" Then .UNIQUAC_R = Double.Parse(node2.InnerText, nf)
                            Case "UNIQUAC_q"
                                If node2.InnerText <> "" Then .UNIQUAC_Q = Double.Parse(node2.InnerText, nf)
                            Case "UNIFAC"
                                .UNIFACGroups.Collection = New SortedList
                                For Each node3 As XmlNode In node2.ChildNodes
                                    .UNIFACGroups.Collection.Add(unif.Group2ID(node3.Attributes("name").InnerText), Integer.Parse(node3.InnerText))
                                Next
                                .MODFACGroups.Collection = New SortedList
                                For Each node3 As XmlNode In node2.ChildNodes
                                    .MODFACGroups.Collection.Add(modf.Group2ID(node3.Attributes("name").InnerText), Integer.Parse(node3.InnerText))
                                Next
                            Case "elements"
                                .Elements.Collection = New SortedList
                                For Each node3 As XmlNode In node2.ChildNodes
                                    .Elements.Collection.Add(node3.Attributes("name").InnerText, Integer.Parse(node3.InnerText))
                                Next
                            Case "COSMODBName"
                                cp.COSMODBName = node2.InnerText
                        End Select
                    Next
                End With

                If CompName = "" Then
                    cpa.Add(cp)
                Else
                    If cp.Name = CompName Then
                        cpa.Add(cp)
                        Exit For
                    End If
                End If

            Next

            unif = Nothing
            modf = Nothing

            Return cpa.ToArray(Type.GetType("DWSIM.DWSIM.ClassesBasicasTermodinamica.ConstantProperties"))

        End Function

    End Class

    Public Class Biodiesel

        Private xmldoc As XmlDocument

        Sub New()

        End Sub

        Public Sub Load(ByVal filename As String)
            Dim pathsep As Char = Path.DirectorySeparatorChar

            Dim settings As New XmlReaderSettings()
            settings.ConformanceLevel = ConformanceLevel.Fragment
            settings.IgnoreWhitespace = True
            settings.IgnoreComments = True
            settings.CheckCharacters = False
            Dim reader As XmlReader = XmlReader.Create(filename)
            reader.Read()

            xmldoc = New XmlDocument
            xmldoc.Load(reader)

        End Sub

        Public Function Transfer(Optional ByVal CompName As String = "") As ClassesBasicasTermodinamica.ConstantProperties()

            Dim cp As ClassesBasicasTermodinamica.ConstantProperties
            Dim cpa As New ArrayList()
            Dim cult As Globalization.CultureInfo = New Globalization.CultureInfo("en-US")
            Dim nf As Globalization.NumberFormatInfo = cult.NumberFormat
            Dim i As Integer = 100000
            For Each node As XmlNode In xmldoc.ChildNodes(1)
                cp = New ClassesBasicasTermodinamica.ConstantProperties
                With cp
                    .OriginalDB = "Biodiesel"
                    For Each node2 As XmlNode In node.ChildNodes
                        Select Case node2.Name
                            Case "Name"
                                .Name = node2.InnerText
                                .Formula = .Name
                            Case "Molecular_Weight"
                                .Molar_Weight = Double.Parse(node2.InnerText, nf)
                            Case "Normal_Boiling_Pt_C"
                                .NBP = Double.Parse(node2.InnerText, nf) + 273.15
                                .Normal_Boiling_Point = .NBP
                            Case "Temperature_C"
                                .Critical_Temperature = Double.Parse(node2.InnerText, nf) + 273.15
                            Case "Pressure_kPa"
                                .Critical_Pressure = Double.Parse(node2.InnerText, nf) * 1000
                            Case "Volume_m3_kgmole"
                                .Critical_Volume = Double.Parse(node2.InnerText, nf)
                            Case "Acentricity"
                                .Acentric_Factor = Double.Parse(node2.InnerText, nf)
                            Case "GS_CS_-_Solubility_Parameter"
                                .Chao_Seader_Solubility_Parameter = Double.Parse(node2.InnerText, nf)
                            Case "GS_CS_-_Mol_Vol_m3_kgmole"
                                .Chao_Seader_Liquid_Molar_Volume = Double.Parse(node2.InnerText, nf)
                            Case "GS_CS_-_Acentricity"
                                .Chao_Seader_Acentricity = Double.Parse(node2.InnerText, nf)
                            Case "UNIQUAC_-_R"
                                .UNIQUAC_R = Double.Parse(node2.InnerText, nf)
                            Case "UNIQUAC_-_Q"
                                .UNIQUAC_Q = Double.Parse(node2.InnerText, nf)
                            Case "Heat_of_Form_25_C_kJ_kgmole"
                                .IG_Enthalpy_of_Formation_25C = Double.Parse(node2.InnerText, nf) / .Molar_Weight
                            Case "Ideal_Gas_Enthalpy_kJ_kg_b"
                                .Ideal_Gas_Heat_Capacity_Const_A = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Enthalpy_kJ_kg_c"
                                .Ideal_Gas_Heat_Capacity_Const_B = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Enthalpy_kJ_kg_d"
                                .Ideal_Gas_Heat_Capacity_Const_C = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Enthalpy_kJ_kg_e"
                                .Ideal_Gas_Heat_Capacity_Const_D = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Enthalpy_kJ_kg_f"
                                .Ideal_Gas_Heat_Capacity_Const_E = Double.Parse(node2.InnerText, nf)
                            Case "Vapor_Pressure_kPa_a"
                                .Vapor_Pressure_Constant_A = Double.Parse(node2.InnerText, nf)
                            Case "Vapor_Pressure_kPa_b"
                                .Vapor_Pressure_Constant_B = Double.Parse(node2.InnerText, nf)
                            Case "Vapor_Pressure_kPa_d"
                                .Vapor_Pressure_Constant_C = Double.Parse(node2.InnerText, nf)
                            Case "Vapor_Pressure_kPa_e"
                                .Vapor_Pressure_Constant_D = Double.Parse(node2.InnerText, nf)
                            Case "Vapor_Pressure_kPa_f"
                                .Vapor_Pressure_Constant_E = Double.Parse(node2.InnerText, nf)
                            Case "IG_Gibbs_Form_kJ_kmol_a"
                                .IG_Gibbs_Energy_of_Formation_25C = Double.Parse(node2.InnerText, nf)
                            Case "IG_Gibbs_Form_kJ_kmol_b"
                                .IG_Gibbs_Energy_of_Formation_25C += Double.Parse(node2.InnerText, nf) * 298.15
                                .IG_Gibbs_Energy_of_Formation_25C /= .Molar_Weight
                                .IG_Entropy_of_Formation_25C = (.IG_Gibbs_Energy_of_Formation_25C - .IG_Enthalpy_of_Formation_25C) / 298.15
                            Case "ZRa"
                                .Z_Rackett = Double.Parse(node2.InnerText, nf)
                        End Select
                    Next
                    .ID = i
                    .IsHYPO = False
                    .IsPF = False
                    .VaporPressureEquation = 101
                    .IdealgasCpEquation = 5
                    .Critical_Compressibility = 0.291 - 0.08 * .Acentric_Factor
                End With

                If CompName = "" Then
                    cpa.Add(cp)
                Else
                    If cp.Name = CompName Then
                        cpa.Add(cp)
                        Exit For
                    End If
                End If

                i += 1
            Next

            Return cpa.ToArray(Type.GetType("DWSIM.DWSIM.ClassesBasicasTermodinamica.ConstantProperties"))

        End Function

    End Class

    <System.Serializable()> Public Class UserDB

        Public Shared Sub CreateNew(ByVal path As String, ByVal TopNode As String)

            Dim writer As New XmlTextWriter(path, Nothing)

            With writer
                .Formatting = Formatting.Indented
                .WriteStartDocument()
                .WriteStartElement(TopNode)
                .WriteEndElement()
                .WriteEndDocument()
                .Flush()
                .Close()
            End With

        End Sub

        Public Shared Sub AddCompounds(ByVal comps() As ClassesBasicasTermodinamica.ConstantProperties, ByVal xmlpath As String, ByVal replace As Boolean)

            Dim xmldoc As XmlDocument
            Dim reader As XmlReader = XmlReader.Create(xmlpath)
            Try
                reader.Read()
            Catch ex As Exception
                reader.Close()
                CreateNew(xmlpath, "compounds")
                reader = XmlReader.Create(xmlpath)
                reader.Read()
            End Try

            Dim cult As Globalization.CultureInfo = New Globalization.CultureInfo("en-US")

            xmldoc = New XmlDocument
            xmldoc.Load(reader)

            For Each comp As ClassesBasicasTermodinamica.ConstantProperties In comps
                Dim index As Integer = -1
                Dim i As Integer = 0
                If xmldoc.ChildNodes.Count > 0 Then
                    For Each node As XmlNode In xmldoc.ChildNodes(1)
                        For Each node2 As XmlNode In node.ChildNodes
                            If node2.Name = "ID" Then
                                If node2.InnerText = comp.ID Then
                                    index = i
                                    Exit For
                                End If
                            End If
                        Next
                        i += 1
                    Next
                End If
                If replace Then
                    If index <> -1 Then xmldoc.ChildNodes(1).RemoveChild(xmldoc.ChildNodes(1).ChildNodes(index))
                End If
                Dim newnode As XmlNode = xmldoc.CreateNode("element", "compound", "")
                With newnode
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Name", "")).InnerText = comp.Name
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "CAS_Number", "")).InnerText = comp.CAS_Number
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "ID", "")).InnerText = comp.ID
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "CompCreatorStudyFile", "")).InnerText = comp.CompCreatorStudyFile
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Formula", "")).InnerText = comp.Formula
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "SMILES", "")).InnerText = comp.SMILES
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Molar_Weight", "")).InnerText = comp.Molar_Weight.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Critical_Temperature", "")).InnerText = comp.Critical_Temperature.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Critical_Pressure", "")).InnerText = comp.Critical_Pressure.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Critical_Volume", "")).InnerText = comp.Critical_Volume.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Critical_Compressibility", "")).InnerText = comp.Critical_Compressibility.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Acentric_Factor", "")).InnerText = comp.Acentric_Factor.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Z_Rackett", "")).InnerText = comp.Z_Rackett.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "PR_Volume_Translation_Coefficient", "")).InnerText = comp.PR_Volume_Translation_Coefficient.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "SRK_Volume_Translation_Coefficient", "")).InnerText = comp.SRK_Volume_Translation_Coefficient.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "CS_Acentric_Factor", "")).InnerText = comp.Chao_Seader_Acentricity.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "CS_Solubility_Parameter", "")).InnerText = comp.Chao_Seader_Solubility_Parameter.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "CS_Liquid_Molar_Volume", "")).InnerText = comp.Chao_Seader_Liquid_Molar_Volume.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "IG_Enthalpy_of_Formation_25C", "")).InnerText = comp.IG_Enthalpy_of_Formation_25C.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "IG_Gibbs_Energy_of_Formation_25C", "")).InnerText = comp.IG_Gibbs_Energy_of_Formation_25C.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Vapor_Pressure_Constant_EqNo", "")).InnerText = comp.VaporPressureEquation
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Vapor_Pressure_Constant_A", "")).InnerText = comp.Vapor_Pressure_Constant_A.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Vapor_Pressure_Constant_B", "")).InnerText = comp.Vapor_Pressure_Constant_B.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Vapor_Pressure_Constant_C", "")).InnerText = comp.Vapor_Pressure_Constant_C.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Vapor_Pressure_Constant_D", "")).InnerText = comp.Vapor_Pressure_Constant_D.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Vapor_Pressure_Constant_E", "")).InnerText = comp.Vapor_Pressure_Constant_E.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Ideal_Gas_Heat_Capacity_EqNo", "")).InnerText = comp.IdealgasCpEquation
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Ideal_Gas_Heat_Capacity_Const_A", "")).InnerText = comp.Ideal_Gas_Heat_Capacity_Const_A.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Ideal_Gas_Heat_Capacity_Const_B", "")).InnerText = comp.Ideal_Gas_Heat_Capacity_Const_B.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Ideal_Gas_Heat_Capacity_Const_C", "")).InnerText = comp.Ideal_Gas_Heat_Capacity_Const_C.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Ideal_Gas_Heat_Capacity_Const_D", "")).InnerText = comp.Ideal_Gas_Heat_Capacity_Const_D.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Ideal_Gas_Heat_Capacity_Const_E", "")).InnerText = comp.Ideal_Gas_Heat_Capacity_Const_E.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Heat_Capacity_EqNo", "")).InnerText = comp.LiquidHeatCapacityEquation
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Heat_Capacity_Const_A", "")).InnerText = comp.Liquid_Heat_Capacity_Const_A.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Heat_Capacity_Const_B", "")).InnerText = comp.Liquid_Heat_Capacity_Const_B.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Heat_Capacity_Const_C", "")).InnerText = comp.Liquid_Heat_Capacity_Const_C.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Heat_Capacity_Const_D", "")).InnerText = comp.Liquid_Heat_Capacity_Const_D.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Heat_Capacity_Const_E", "")).InnerText = comp.Liquid_Heat_Capacity_Const_E.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Heat_Capacity_TMin", "")).InnerText = comp.Liquid_Heat_Capacity_Tmin.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Heat_Capacity_TMax", "")).InnerText = comp.Liquid_Heat_Capacity_Tmax.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Viscosity_EqNo", "")).InnerText = comp.LiquidViscosityEquation
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Viscosity_Const_A", "")).InnerText = comp.Liquid_Viscosity_Const_A.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Viscosity_Const_B", "")).InnerText = comp.Liquid_Viscosity_Const_B.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Viscosity_Const_C", "")).InnerText = comp.Liquid_Viscosity_Const_C.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Viscosity_Const_D", "")).InnerText = comp.Liquid_Viscosity_Const_D.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Viscosity_Const_E", "")).InnerText = comp.Liquid_Viscosity_Const_E.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Density_EqNo", "")).InnerText = comp.LiquidDensityEquation
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Density_Const_A", "")).InnerText = comp.Liquid_Density_Const_A.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Density_Const_B", "")).InnerText = comp.Liquid_Density_Const_B.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Density_Const_C", "")).InnerText = comp.Liquid_Density_Const_C.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Density_Const_D", "")).InnerText = comp.Liquid_Density_Const_D.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Density_Const_E", "")).InnerText = comp.Liquid_Density_Const_E.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Solid_Heat_Capacity_EqNo", "")).InnerText = comp.SolidHeatCapacityEquation
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Solid_Heat_Capacity_Constant_A", "")).InnerText = comp.Solid_Heat_Capacity_Const_A.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Solid_Heat_Capacity_Constant_B", "")).InnerText = comp.Solid_Heat_Capacity_Const_B.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Solid_Heat_Capacity_Constant_C", "")).InnerText = comp.Solid_Heat_Capacity_Const_C.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Solid_Heat_Capacity_Constant_D", "")).InnerText = comp.Solid_Heat_Capacity_Const_D.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Solid_Heat_Capacity_Constant_E", "")).InnerText = comp.Solid_Heat_Capacity_Const_E.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Solid_Heat_Capacity_TMin", "")).InnerText = comp.Solid_Heat_Capacity_Tmin.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Solid_Heat_Capacity_TMax", "")).InnerText = comp.Solid_Heat_Capacity_Tmax.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Solid_Density_EqNo", "")).InnerText = comp.SolidDensityEquation
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Solid_Density_Constant_A", "")).InnerText = comp.Solid_Density_Const_A.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Solid_Density_Constant_B", "")).InnerText = comp.Solid_Density_Const_B.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Solid_Density_Constant_C", "")).InnerText = comp.Solid_Density_Const_C.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Solid_Density_Constant_D", "")).InnerText = comp.Solid_Density_Const_D.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Solid_Density_Constant_E", "")).InnerText = comp.Solid_Density_Const_E.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Solid_Density_TMin", "")).InnerText = comp.Solid_Density_Tmin.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Solid_Density_TMax", "")).InnerText = comp.Solid_Density_Tmax.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Normal_Boiling_Point", "")).InnerText = comp.Normal_Boiling_Point.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "UNIQUAC_q", "")).InnerText = comp.UNIQUAC_R.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "UNIQUAC_r", "")).InnerText = comp.UNIQUAC_Q.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Temperature_of_fusion", "")).InnerText = comp.TemperatureOfFusion.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Enthalpy_of_fusion", "")).InnerText = comp.EnthalpyOfFusionAtTf.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "PC_SAFT_sigma", "")).InnerText = comp.PC_SAFT_sigma.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "PC_SAFT_m", "")).InnerText = comp.PC_SAFT_m.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "PC_SAFT_epsilon_k", "")).InnerText = comp.PC_SAFT_epsilon_k.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "IsBlackOil", "")).InnerText = comp.IsBlackOil
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "BlackOil_GOR", "")).InnerText = comp.BO_GOR.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "BlackOil_BSW", "")).InnerText = comp.BO_BSW.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "BlackOil_SGG", "")).InnerText = comp.BO_SGG.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "BlackOil_SGO", "")).InnerText = comp.BO_SGO.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "BlackOil_OilVisc1", "")).InnerText = comp.BO_OilVisc1.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "BlackOil_OilVisc2", "")).InnerText = comp.BO_OilVisc2.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "BlackOil_OilViscTemp1", "")).InnerText = comp.BO_OilViscTemp1.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "BlackOil_OilViscTemp2", "")).InnerText = comp.BO_OilViscTemp2.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "BlackOil_PNA_A", "")).InnerText = comp.BO_PNA_A.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "BlackOil_PNA_N", "")).InnerText = comp.BO_PNA_N.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "BlackOil_PNA_P", "")).InnerText = comp.BO_PNA_P.ToString(cult)

                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Ion", "")).InnerText = comp.IsIon
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Salt", "")).InnerText = comp.IsSalt
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "HydratedSalt", "")).InnerText = comp.IsHydratedSalt

                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "HydrationNumber", "")).InnerText = comp.HydrationNumber.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Charge", "")).InnerText = comp.Charge.ToString(cult)
               
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "PositiveIon", "")).InnerText = comp.PositiveIon
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "NegativeIon", "")).InnerText = comp.NegativeIon

                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "PositiveIonStoichCoeff", "")).InnerText = comp.PositiveIonStoichCoeff.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "NegativeIonStoichCoeff", "")).InnerText = comp.NegativeIonStoichCoeff.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "StoichSum", "")).InnerText = comp.StoichSum.ToString(cult)

                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "DelGF_kJ_mol", "")).InnerText = comp.Electrolyte_DelGF.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "DelHf_kJ_mol", "")).InnerText = comp.Electrolyte_DelHF.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Cp_J_mol_K", "")).InnerText = comp.Electrolyte_Cp0.ToString(cult)

                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Tf_C", "")).InnerText = comp.TemperatureOfFusion.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Hfus_at_Tf_kJ_mol", "")).InnerText = comp.EnthalpyOfFusionAtTf.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "DenS_T_C", "")).InnerText = comp.SolidTs.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "DenS_g_mL", "")).InnerText = comp.SolidDensityAtTs.ToString(cult)

                    If comp.UNIFACGroups.Collection.Count > 0 Then
                        With .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "UNIFAC", ""))
                            For Each kvp As DictionaryEntry In comp.UNIFACGroups.Collection
                                .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "", "UNIFACGroup", "")).InnerText = kvp.Value
                                .ChildNodes(.ChildNodes.Count - 1).Attributes.Append(xmldoc.CreateAttribute("ID"))
                                .ChildNodes(.ChildNodes.Count - 1).Attributes("ID").Value = kvp.Key
                            Next
                        End With
                    End If
                    
                    If comp.MODFACGroups.Collection.Count > 0 Then
                        With .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "MODFAC", ""))
                            For Each kvp As DictionaryEntry In comp.MODFACGroups.Collection
                                .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "", "MODFACGroup", "")).InnerText = kvp.Value
                                .ChildNodes(.ChildNodes.Count - 1).Attributes.Append(xmldoc.CreateAttribute("ID"))
                                .ChildNodes(.ChildNodes.Count - 1).Attributes("ID").Value = kvp.Key
                            Next
                        End With
                    End If
                    
                    If comp.NISTMODFACGroups.Collection.Count > 0 Then
                        With .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "NISTMODFAC", ""))
                            For Each kvp As DictionaryEntry In comp.NISTMODFACGroups.Collection
                                .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "", "NISTMODFACGroup", "")).InnerText = kvp.Value
                                .ChildNodes(.ChildNodes.Count - 1).Attributes.Append(xmldoc.CreateAttribute("ID"))
                                .ChildNodes(.ChildNodes.Count - 1).Attributes("ID").Value = kvp.Key
                            Next
                        End With
                    End If
                    With .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "elements", ""))
                        For Each el As DictionaryEntry In comp.Elements.Collection
                            .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "", "element", "")).InnerText = el.Value
                            .ChildNodes(.ChildNodes.Count - 1).Attributes.Append(xmldoc.CreateAttribute("name"))
                            .ChildNodes(.ChildNodes.Count - 1).Attributes("name").Value = el.Key
                        Next
                    End With
                End With
                xmldoc.ChildNodes(1).AppendChild(newnode)
            Next

            reader.Close()
            reader = Nothing

            xmldoc.Save(xmlpath)
            xmldoc = Nothing


        End Sub

        Public Shared Sub RemoveCompound(ByVal xmlpath As String, ByVal compID As String)

            Dim xmldoc As XmlDocument
            Dim reader As XmlReader = XmlReader.Create(xmlpath)
            reader.Read()

            xmldoc = New XmlDocument
            xmldoc.Load(reader)

            Dim index As Integer = -1
            Dim i As Integer = 0
            For Each node As XmlNode In xmldoc.ChildNodes(1)
                For Each node2 As XmlNode In node.ChildNodes
                    If node2.Name = "ID" Then
                        If node2.InnerText = compID Then
                            index = i
                            Exit For
                        End If
                    End If
                Next
                i += 1
            Next

            If index <> -1 Then xmldoc.ChildNodes(1).RemoveChild(xmldoc.ChildNodes(1).ChildNodes(index))


            reader.Close()
            reader = Nothing

            xmldoc.Save(xmlpath)
            xmldoc = Nothing

        End Sub

        Public Shared Function ReadComps(ByVal xmlpath As String) As ClassesBasicasTermodinamica.ConstantProperties()

            Dim xmldoc As XmlDocument
            Dim reader As XmlReader = XmlReader.Create(xmlpath)
            reader.Read()

            xmldoc = New XmlDocument
            xmldoc.Load(reader)

            Dim cp As ClassesBasicasTermodinamica.ConstantProperties
            Dim cpa As New ArrayList()
            Dim cult As Globalization.CultureInfo = New Globalization.CultureInfo("en-US")
            Dim nf As Globalization.NumberFormatInfo = cult.NumberFormat

            For Each node As XmlNode In xmldoc.ChildNodes(1)
                cp = New ClassesBasicasTermodinamica.ConstantProperties
                With cp
                    .OriginalDB = "User"
                    .CurrentDB = "User"
                    For Each node2 As XmlNode In node.ChildNodes
                        Select Case node2.Name
                            Case "Name"
                                .Name = node2.InnerText
                            Case "CAS_Number"
                                .CAS_Number = node2.InnerText
                            Case "CompCreatorStudyFile"
                                .CompCreatorStudyFile = node2.InnerText
                            Case "Formula"
                                .Formula = node2.InnerText
                            Case "Molar_Weight"
                                .Molar_Weight = Double.Parse(node2.InnerText, nf)
                            Case "Critical_Temperature"
                                .Critical_Temperature = Double.Parse(node2.InnerText, nf)
                            Case "Critical_Pressure"
                                .Critical_Pressure = Double.Parse(node2.InnerText, nf)
                            Case "Critical_Volume"
                                .Critical_Volume = Double.Parse(node2.InnerText, nf)
                            Case "Critical_Compressibility"
                                .Critical_Compressibility = Double.Parse(node2.InnerText, nf)
                            Case "Acentric_Factor"
                                .Acentric_Factor = Double.Parse(node2.InnerText, nf)
                            Case "Z_Rackett"
                                .Z_Rackett = Double.Parse(node2.InnerText, nf)
                            Case "PR_Volume_Translation_Coefficient"
                                .PR_Volume_Translation_Coefficient = Double.Parse(node2.InnerText, nf)
                            Case "SRK_Volume_Translation_Coefficient"
                                .SRK_Volume_Translation_Coefficient = Double.Parse(node2.InnerText, nf)
                            Case "CS_Acentric_Factor"
                                .Chao_Seader_Acentricity = Double.Parse(node2.InnerText, nf)
                            Case "CS_Solubility_Parameter"
                                .Chao_Seader_Solubility_Parameter = Double.Parse(node2.InnerText, nf)
                            Case "CS_Liquid_Molar_Volume"
                                .Chao_Seader_Liquid_Molar_Volume = Double.Parse(node2.InnerText, nf)
                            Case "IG_Enthalpy_of_Formation_25C"
                                .IG_Enthalpy_of_Formation_25C = Double.Parse(node2.InnerText, nf)
                            Case "IG_Gibbs_Energy_of_Formation_25C"
                                .IG_Gibbs_Energy_of_Formation_25C = Double.Parse(node2.InnerText, nf)
                            Case "Vapor_Pressure_Constant_EqNo"
                                If node2.InnerText <> "" Then .VaporPressureEquation = Integer.Parse(node2.InnerText, nf)
                            Case "Vapor_Pressure_Constant_A"
                                .Vapor_Pressure_Constant_A = Double.Parse(node2.InnerText, nf)
                            Case "Vapor_Pressure_Constant_B"
                                .Vapor_Pressure_Constant_B = Double.Parse(node2.InnerText, nf)
                            Case "Vapor_Pressure_Constant_C"
                                .Vapor_Pressure_Constant_C = Double.Parse(node2.InnerText, nf)
                            Case "Vapor_Pressure_Constant_D"
                                .Vapor_Pressure_Constant_D = Double.Parse(node2.InnerText, nf)
                            Case "Vapor_Pressure_Constant_E"
                                .Vapor_Pressure_Constant_E = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Heat_Capacity_EqNo"
                                If node2.InnerText <> "" Then .IdealgasCpEquation = Integer.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Heat_Capacity_Const_A"
                                .Ideal_Gas_Heat_Capacity_Const_A = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Heat_Capacity_Const_B"
                                .Ideal_Gas_Heat_Capacity_Const_B = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Heat_Capacity_Const_C"
                                .Ideal_Gas_Heat_Capacity_Const_C = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Heat_Capacity_Const_D"
                                .Ideal_Gas_Heat_Capacity_Const_D = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Heat_Capacity_Const_E"
                                .Ideal_Gas_Heat_Capacity_Const_E = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Viscosity_Const_EqNo"
                                If node2.InnerText <> "" Then .LiquidViscosityEquation = Integer.Parse(node2.InnerText, nf)
                            Case "Liquid_Viscosity_Const_A"
                                .Liquid_Viscosity_Const_A = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Viscosity_Const_B"
                                .Liquid_Viscosity_Const_B = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Viscosity_Const_C"
                                .Liquid_Viscosity_Const_C = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Viscosity_Const_D"
                                .Liquid_Viscosity_Const_D = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Viscosity_Const_E"
                                .Liquid_Viscosity_Const_E = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Density_Const_EqNo"
                                If node2.InnerText <> "" Then .LiquidDensityEquation = Integer.Parse(node2.InnerText, nf)
                            Case "Liquid_Density_Const_A"
                                .Liquid_Density_Const_A = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Density_Const_B"
                                .Liquid_Density_Const_B = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Density_Const_C"
                                .Liquid_Density_Const_C = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Density_Const_D"
                                .Liquid_Density_Const_D = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Density_Const_E"
                                .Liquid_Density_Const_E = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Heat_Capacity_EqNo"
                                If node2.InnerText <> "" Then .LiquidHeatCapacityEquation = Integer.Parse(node2.InnerText, nf)
                            Case "Liquid_Heat_Capacity_Const_A"
                                .Liquid_Heat_Capacity_Const_A = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Heat_Capacity_Const_B"
                                .Liquid_Heat_Capacity_Const_B = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Heat_Capacity_Const_C"
                                .Liquid_Heat_Capacity_Const_C = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Heat_Capacity_Const_D"
                                .Liquid_Heat_Capacity_Const_D = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Heat_Capacity_Const_E"
                                .Liquid_Heat_Capacity_Const_E = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Heat_Capacity_TMin"
                                .Liquid_Heat_Capacity_Tmin = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Heat_Capacity_TMax"
                                .Liquid_Heat_Capacity_Tmax = Double.Parse(node2.InnerText, nf)
                            Case "Solid_Heat_Capacity_EqNo"
                                If node2.InnerText <> "" Then .SolidHeatCapacityEquation = Integer.Parse(node2.InnerText, nf)
                            Case "Solid_Heat_Capacity_Constant_A"
                                .Solid_Heat_Capacity_Const_A = Double.Parse(node2.InnerText, nf)
                            Case "Solid_Heat_Capacity_Constant_B"
                                .Solid_Heat_Capacity_Const_B = Double.Parse(node2.InnerText, nf)
                            Case "Solid_Heat_Capacity_Constant_C"
                                .Solid_Heat_Capacity_Const_C = Double.Parse(node2.InnerText, nf)
                            Case "Solid_Heat_Capacity_Constant_D"
                                .Solid_Heat_Capacity_Const_D = Double.Parse(node2.InnerText, nf)
                            Case "Solid_Heat_Capacity_Constant_E"
                                .Solid_Heat_Capacity_Const_E = Double.Parse(node2.InnerText, nf)
                            Case "Solid_Heat_Capacity_TMin"
                                .Solid_Heat_Capacity_Tmin = Double.Parse(node2.InnerText, nf)
                            Case "Solid_Heat_Capacity_TMax"
                                .Solid_Heat_Capacity_Tmax = Double.Parse(node2.InnerText, nf)
                            Case "Solid_Density_EqNo"
                                If node2.InnerText <> "" Then .SolidDensityEquation = Integer.Parse(node2.InnerText, nf)
                            Case "Solid_Density_Constant_A"
                                .Solid_Density_Const_A = Double.Parse(node2.InnerText, nf)
                            Case "Solid_Density_Constant_B"
                                .Solid_Density_Const_B = Double.Parse(node2.InnerText, nf)
                            Case "Solid_Density_Constant_C"
                                .Solid_Density_Const_C = Double.Parse(node2.InnerText, nf)
                            Case "Solid_Density_Constant_D"
                                .Solid_Density_Const_D = Double.Parse(node2.InnerText, nf)
                            Case "Solid_Density_Constant_E"
                                .Solid_Density_Const_E = Double.Parse(node2.InnerText, nf)
                            Case "Solid_Density_TMin"
                                .Solid_Density_Tmin = Double.Parse(node2.InnerText, nf)
                            Case "Solid_Density_TMax"
                                .Solid_Density_Tmax = Double.Parse(node2.InnerText, nf)
                            Case "Normal_Boiling_Point"
                                .Normal_Boiling_Point = Double.Parse(node2.InnerText, nf)
                            Case "Temperature_of_fusion"
                                .TemperatureOfFusion = Double.Parse(node2.InnerText, nf)
                            Case "Enthalpy_of_fusion"
                                .EnthalpyOfFusionAtTf = Double.Parse(node2.InnerText, nf)
                            Case "SMILES"
                                .SMILES = node2.InnerText
                            Case "ID"
                                .ID = Integer.Parse(node2.InnerText)
                            Case "UNIQUAC_r"
                                If node2.InnerText <> "" Then .UNIQUAC_R = Double.Parse(node2.InnerText, nf)
                            Case "UNIQUAC_q"
                                If node2.InnerText <> "" Then .UNIQUAC_Q = Double.Parse(node2.InnerText, nf)
                            Case "UNIFAC"
                                .UNIFACGroups.Collection = New SortedList
                                Dim unif As New SimulationObjects.PropertyPackages.Auxiliary.Unifac
                                Dim GID, nid As String
                                For Each node3 As XmlNode In node2.ChildNodes
                                    nid = node3.Attributes.Item(0).Name
                                    If node3.Attributes.Item(0).Name = "ID" Then
                                        .UNIFACGroups.Collection.Add(node3.Attributes("ID").InnerText, Integer.Parse(node3.InnerText))
                                    Else 'read data of old file format
                                        GID = unif.Group2ID(node3.Attributes("name").InnerText)
                                        .UNIFACGroups.Collection.Add(GID, Integer.Parse(node3.InnerText))
                                    End If
                                Next
                            Case "MODFAC"
                                .MODFACGroups.Collection = New SortedList
                                Dim modf As New SimulationObjects.PropertyPackages.Auxiliary.Modfac
                                Dim GID As String
                                For Each node3 As XmlNode In node2.ChildNodes
                                    If node3.Attributes.Item(0).Name = "ID" Then
                                        .MODFACGroups.Collection.Add(node3.Attributes("ID").InnerText, Integer.Parse(node3.InnerText))
                                    Else 'read data of old file format
                                        GID = modf.Group2ID(node3.Attributes("name").InnerText)
                                        .MODFACGroups.Collection.Add(node3.Attributes("name").InnerText, Integer.Parse(node3.InnerText))
                                    End If
                                Next
                            Case "NISTMODFAC"
                                .NISTMODFACGroups.Collection = New SortedList
                                Dim nimodf As New SimulationObjects.PropertyPackages.Auxiliary.NISTMFAC
                                Dim GID As String
                                For Each node3 As XmlNode In node2.ChildNodes
                                    If node3.Attributes.Item(0).Name = "ID" Then
                                        .NISTMODFACGroups.Collection.Add(node3.Attributes("ID").InnerText, Integer.Parse(node3.InnerText))
                                    Else 'read data of old file format
                                        GID = nimodf.Group2ID(node3.Attributes("name").InnerText)
                                        .NISTMODFACGroups.Collection.Add(node3.Attributes("name").InnerText, Integer.Parse(node3.InnerText))
                                    End If
                                Next
                            Case "PC_SAFT_sigma"
                                .PC_SAFT_sigma = Double.Parse(node2.InnerText, nf)
                            Case "PC_SAFT_m"
                                .PC_SAFT_m = Double.Parse(node2.InnerText, nf)
                            Case "PC_SAFT_epsilon_k"
                                .PC_SAFT_epsilon_k = Double.Parse(node2.InnerText, nf)
                            Case "elements"
                                .Elements.Collection = New SortedList
                                For Each node3 As XmlNode In node2.ChildNodes
                                    .Elements.Collection.Add(node3.Attributes("name").InnerText, Integer.Parse(node3.InnerText))
                                Next
                            Case "BlackOil_GOR"
                                .BO_GOR = Double.Parse(node2.InnerText, nf)
                            Case "BlackOil_BSW"
                                .BO_BSW = Double.Parse(node2.InnerText, nf)
                            Case "BlackOil_SGG"
                                .BO_SGG = Double.Parse(node2.InnerText, nf)
                            Case "BlackOil_SGO"
                                .BO_SGO = Double.Parse(node2.InnerText, nf)
                            Case "BlackOil_OilVisc1"
                                .BO_OilVisc1 = Double.Parse(node2.InnerText, nf)
                            Case "BlackOil_OilVisc2"
                                .BO_OilVisc2 = Double.Parse(node2.InnerText, nf)
                            Case "BlackOil_OilViscTemp1"
                                .BO_OilViscTemp1 = Double.Parse(node2.InnerText, nf)
                            Case "BlackOil_OilViscTemp2"
                                .BO_OilViscTemp2 = Double.Parse(node2.InnerText, nf)
                            Case "BlackOil_PNA_A"
                                .BO_PNA_A = Double.Parse(node2.InnerText, nf)
                            Case "BlackOil_PNA_N"
                                .BO_PNA_N = Double.Parse(node2.InnerText, nf)
                            Case "BlackOil_PNA_P"
                                .BO_PNA_P = Double.Parse(node2.InnerText, nf)
                            Case "IsBlackOil"
                                .IsBlackOil = Boolean.Parse(node2.InnerText)

                                'electrolyte

                            Case "Ion"
                                .IsIon = node2.InnerText
                            Case "Salt"
                                .IsSalt = node2.InnerText
                            Case "HydratedSalt"
                                .IsHydratedSalt = node2.InnerText
                            Case "HydrationNumber"
                                .HydrationNumber = Double.Parse(node2.InnerText, nf)
                            Case "Charge"
                                .Charge = node2.InnerText
                            Case "PositiveIon"
                                .PositiveIon = node2.InnerText
                            Case "NegativeIon"
                                .NegativeIon = node2.InnerText
                            Case "PositiveIonStoichCoeff"
                                .PositiveIonStoichCoeff = node2.InnerText
                            Case "NegativeIonStoichCoeff"
                                .NegativeIonStoichCoeff = node2.InnerText
                            Case "StoichSum"
                                .StoichSum = node2.InnerText
                            Case "DelGF_kJ_mol"
                                .Electrolyte_DelGF = Double.Parse(node2.InnerText, nf) 'kJ/mol
                                .IG_Gibbs_Energy_of_Formation_25C = Double.Parse(node2.InnerText, nf) * 1000 / .Molar_Weight 'kJ/kg
                            Case "DelHf_kJ_mol"
                                .Electrolyte_DelHF = Double.Parse(node2.InnerText, nf) 'kJ/mol
                                .IG_Enthalpy_of_Formation_25C = Double.Parse(node2.InnerText, nf) * 1000 / .Molar_Weight 'kJ/kg
                            Case "Cp_J_mol_K"
                                .Electrolyte_Cp0 = Double.Parse(node2.InnerText, nf) 'kJ/mol.K
                            Case "Tf_C"
                                .TemperatureOfFusion = Double.Parse(node2.InnerText, nf) 'K
                            Case "Hfus_at_Tf_kJ_mol"
                                .EnthalpyOfFusionAtTf = Double.Parse(node2.InnerText, nf) 'kJ/mol
                            Case "DenS_T_C"
                                .SolidTs = Double.Parse(node2.InnerText, nf) 'K
                            Case "DenS_g_mL"
                                .SolidDensityAtTs = Double.Parse(node2.InnerText, nf)  'kg/m3

                        End Select
                    Next
                End With
                cpa.Add(cp)
            Next

            xmldoc = Nothing

            reader.Close()
            reader = Nothing

            Return cpa.ToArray(Type.GetType("DWSIM.DWSIM.ClassesBasicasTermodinamica.ConstantProperties"))

        End Function

    End Class

    ''' <summary>
    ''' Interaction Parameter user database class
    ''' </summary>
    ''' <remarks></remarks>
    <System.Serializable()> Public Class UserIPDB

        Public Shared Sub CreateNew(ByVal path As String, ByVal TopNode As String)

            Dim writer As New XmlTextWriter(path, Nothing)

            With writer
                .Formatting = Formatting.Indented
                .WriteStartDocument()
                .WriteStartElement(TopNode)
                .WriteEndElement()
                .WriteEndDocument()
                .Flush()
                .Close()
            End With

        End Sub

        Public Shared Sub AddInteractionParameters(ByVal IPDS() As ClassesBasicasTermodinamica.InteractionParameter, ByVal xmlpath As String, ByVal replace As Boolean)
            Dim xmldoc As XmlDocument
            Dim reader As XmlReader = XmlReader.Create(xmlpath)
            Try
                reader.Read()
            Catch ex As Exception
                reader.Close()
                CreateNew(xmlpath, "Interactions")
                reader = XmlReader.Create(xmlpath)
                reader.Read()
            End Try

            Dim cult As Globalization.CultureInfo = New Globalization.CultureInfo("en-US")
            Dim p As Double

            xmldoc = New XmlDocument
            xmldoc.Load(reader)

            For Each IP As ClassesBasicasTermodinamica.InteractionParameter In IPDS
                Dim index As Integer = -1
                Dim i As Integer = 0
                Dim C1, C2, M, S1, S2 As String

                If xmldoc.ChildNodes.Count > 0 Then
                    For Each node As XmlNode In xmldoc.ChildNodes(1)
                        C1 = ""
                        C2 = ""
                        M = ""
                        For Each node2 As XmlNode In node.ChildNodes
                            If node2.Name = "Comp1" Then C1 = node2.InnerText
                            If node2.Name = "Comp2" Then C2 = node2.InnerText
                            If node2.Name = "Model" Then M = node2.InnerText
                            S1 = C1 & C2 & M
                            S2 = C2 & C1 & M
                            If (S1 = IP.Comp1 & IP.Comp2 & IP.Model) Or (S2 = IP.Comp1 & IP.Comp2 & IP.Model) Then
                                index = i
                                Exit For
                            End If
                        Next
                        If index <> -1 Then Exit For
                        i += 1
                    Next
                End If
                If replace Then
                    If index <> -1 Then xmldoc.ChildNodes(1).RemoveChild(xmldoc.ChildNodes(1).ChildNodes(index))
                End If

                Dim newnode As XmlNode = xmldoc.CreateNode("element", "Interaction", "")
                With newnode
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Comp1", "")).InnerText = IP.Comp1
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Comp2", "")).InnerText = IP.Comp2
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Model", "")).InnerText = IP.Model
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "DataType", "")).InnerText = IP.DataType
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Description", "")).InnerText = IP.Description
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "RegressionFile", "")).InnerText = IP.RegressionFile
                    With .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Parameters", ""))
                        For Each par As KeyValuePair(Of String, Object) In IP.Parameters
                            p = par.Value
                            .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "", "Parameter", "")).InnerText = p.ToString(cult)
                            .ChildNodes(.ChildNodes.Count - 1).Attributes.Append(xmldoc.CreateAttribute("name"))
                            .ChildNodes(.ChildNodes.Count - 1).Attributes("name").Value = par.Key
                        Next
                    End With

                End With
                xmldoc.ChildNodes(1).AppendChild(newnode)
            Next

            reader.Close()
            reader = Nothing

            xmldoc.Save(xmlpath)
            xmldoc = Nothing
        End Sub

        Public Shared Function ReadInteractions(ByVal xmlpath As String, ByVal Model As String) As ClassesBasicasTermodinamica.InteractionParameter()

            Dim xmldoc As XmlDocument
            Dim reader As XmlReader = XmlReader.Create(xmlpath)
            reader.Read()

            xmldoc = New XmlDocument
            xmldoc.Load(reader)

            Dim IP As ClassesBasicasTermodinamica.InteractionParameter
            Dim IPA As New ArrayList()
            Dim cult As Globalization.CultureInfo = New Globalization.CultureInfo("en-US")

            For Each node As XmlNode In xmldoc.ChildNodes(1)
                IP = New ClassesBasicasTermodinamica.InteractionParameter
                With IP
                    For Each node2 As XmlNode In node.ChildNodes
                        Select Case node2.Name
                            Case "Comp1"
                                .Comp1 = node2.InnerText
                            Case "Comp2"
                                .Comp2 = node2.InnerText
                            Case "Model"
                                .Model = node2.InnerText
                            Case "DataType"
                                .DataType = node2.InnerText
                            Case "Description"
                                .Description = node2.InnerText
                            Case "RegressionFile"
                                .RegressionFile = node2.InnerText
                            Case "Parameters"
                                For Each node3 As XmlNode In node2.ChildNodes
                                    .Parameters.Add(node3.Attributes("name").InnerText, Double.Parse(node3.InnerText, cult))
                                Next
                        End Select
                    Next
                End With

                If IP.Model = Model Then
                    IPA.Add(IP)
                End If

            Next

            xmldoc = Nothing

            reader.Close()
            reader = Nothing

            Return IPA.ToArray(Type.GetType("DWSIM.DWSIM.ClassesBasicasTermodinamica.InteractionParameter"))

        End Function

        Public Shared Function GetStoredIPsets(ByVal comp1 As String, ByVal comp2 As String, ByVal model As String) As List(Of ClassesBasicasTermodinamica.InteractionParameter)

            Dim results As New List(Of ClassesBasicasTermodinamica.InteractionParameter)

            If Not My.Settings.UserInteractionsDatabases Is Nothing Then
                For Each IPDBPath As String In My.Settings.UserInteractionsDatabases
                    Dim Interactions As ClassesBasicasTermodinamica.InteractionParameter()
                    Try
                        Interactions = UserIPDB.ReadInteractions(IPDBPath, model)
                        For Each ipset As ClassesBasicasTermodinamica.InteractionParameter In Interactions
                            If ipset.Comp1 = comp1 And ipset.Comp2 = comp2 Then
                                results.Add(ipset)
                            ElseIf ipset.Comp1 = comp2 And ipset.Comp2 = comp1 Then
                                'invert parameters
                                Dim ipset2 As ClassesBasicasTermodinamica.InteractionParameter = ipset.Clone
                                Dim tmpval As Object
                                With ipset2
                                    .Comp1 = comp2
                                    .Comp2 = comp1
                                    If .Parameters.ContainsKey("A12") Then
                                        tmpval = .Parameters("A12")
                                        .Parameters("A12") = .Parameters("A21")
                                        .Parameters("A21") = tmpval
                                    End If
                                    If .Parameters.ContainsKey("B12") Then
                                        tmpval = .Parameters("B12")
                                        .Parameters("B12") = .Parameters("B21")
                                        .Parameters("B21") = tmpval
                                    End If
                                    If .Parameters.ContainsKey("C12") Then
                                        tmpval = .Parameters("C12")
                                        .Parameters("C12") = .Parameters("C21")
                                        .Parameters("C21") = tmpval
                                    End If
                                    If .Parameters.ContainsKey("kji") Then
                                        tmpval = .Parameters("kij")
                                        .Parameters("kij") = .Parameters("kji")
                                        .Parameters("kji") = tmpval
                                    End If
                                End With
                                results.Add(ipset2)
                            End If
                        Next
                    Catch ex As Exception
                    End Try
                Next
            End If
            Return results
        End Function

    End Class

    Public Class Electrolyte

        Private xmldoc As XmlDocument

        Sub New()

        End Sub

        Public Sub Load(ByVal filename As String)
            Dim pathsep As Char = Path.DirectorySeparatorChar

            Dim settings As New XmlReaderSettings()
            settings.ConformanceLevel = ConformanceLevel.Fragment
            settings.IgnoreWhitespace = True
            settings.IgnoreComments = True
            settings.CheckCharacters = False
            Dim reader As XmlReader = XmlReader.Create(filename)
            reader.Read()

            xmldoc = New XmlDocument
            xmldoc.Load(reader)

        End Sub

        Public Function Transfer(Optional ByVal CompName As String = "") As ClassesBasicasTermodinamica.ConstantProperties()

            Dim cp As ClassesBasicasTermodinamica.ConstantProperties
            Dim cpa As New ArrayList()
            Dim cult As Globalization.CultureInfo = New Globalization.CultureInfo("en-US")
            Dim nf As Globalization.NumberFormatInfo = cult.NumberFormat
            Dim i As Integer = 200000
            For Each node As XmlNode In xmldoc.ChildNodes(1)
                cp = New ClassesBasicasTermodinamica.ConstantProperties
                With cp
                    .OriginalDB = "Electrolytes"
                    For Each node2 As XmlNode In node.ChildNodes
                        Select Case node2.Name
                            Case "Name"
                                .Name = node2.InnerText
                            Case "Formula"
                                .Formula = node2.InnerText
                            Case "MW"
                                .Molar_Weight = Double.Parse(node2.InnerText, nf)
                            Case "Ion"
                                .IsIon = node2.InnerText
                            Case "Salt"
                                .IsSalt = node2.InnerText
                            Case "HydratedSalt"
                                .IsHydratedSalt = node2.InnerText
                            Case "HydrationNumber"
                                .HydrationNumber = Double.Parse(node2.InnerText, nf)
                            Case "Charge"
                                .Charge = node2.InnerText
                            Case "PositiveIon"
                                .PositiveIon = node2.InnerText
                            Case "NegativeIon"
                                .NegativeIon = node2.InnerText
                            Case "PositiveIonStoichCoeff"
                                .PositiveIonStoichCoeff = node2.InnerText
                            Case "NegativeIonStoichCoeff"
                                .NegativeIonStoichCoeff = node2.InnerText
                            Case "StoichSum"
                                .StoichSum = node2.InnerText
                            Case "DelGF_kJ_mol"
                                .Electrolyte_DelGF = Double.Parse(node2.InnerText, nf) 'kJ/mol
                                .IG_Gibbs_Energy_of_Formation_25C = Double.Parse(node2.InnerText, nf) * 1000 / .Molar_Weight 'kJ/kg
                            Case "DelHf_kJ_mol"
                                .Electrolyte_DelHF = Double.Parse(node2.InnerText, nf) 'kJ/mol
                                .IG_Enthalpy_of_Formation_25C = Double.Parse(node2.InnerText, nf) * 1000 / .Molar_Weight 'kJ/kg
                            Case "Cp_J_mol_K"
                                .Electrolyte_Cp0 = Double.Parse(node2.InnerText, nf) / 1000 'kJ/mol.K
                            Case "Tf_C"
                                .TemperatureOfFusion = Double.Parse(node2.InnerText, nf) + 273.15 'K
                            Case "Hfus_at_Tf_kJ_mol"
                                .EnthalpyOfFusionAtTf = Double.Parse(node2.InnerText, nf) 'kJ/mol
                            Case "DenS_T_C"
                                .SolidTs = Double.Parse(node2.InnerText, nf) + 273.15 'K
                            Case "DenS_g_mL"
                                .SolidDensityAtTs = Double.Parse(node2.InnerText, nf) * 1000 'kg/m3
                            Case "StandardStateMolarVolume_cm3_mol"
                                .StandardStateMolarVolume = Double.Parse(node2.InnerText, nf)
                            Case "MolarVolume_v2i"
                                .MolarVolume_v2i = Double.Parse(node2.InnerText, nf)
                            Case "MolarVolume_v3i"
                                .MolarVolume_v3i = Double.Parse(node2.InnerText, nf)
                            Case "MolarVolume_k1i"
                                .MolarVolume_k1i = Double.Parse(node2.InnerText, nf)
                            Case "MolarVolume_k2i"
                                .MolarVolume_k2i = Double.Parse(node2.InnerText, nf)
                            Case "MolarVolume_k3i"
                                .MolarVolume_k3i = Double.Parse(node2.InnerText, nf)
                            Case "CpAq_a"
                                .Ion_CpAq_a = Double.Parse(node2.InnerText, nf)
                            Case "CpAq_b"
                                .Ion_CpAq_b = Double.Parse(node2.InnerText, nf)
                            Case "CpAq_c"
                                .Ion_CpAq_c = Double.Parse(node2.InnerText, nf)
                        End Select
                    Next
                    .ID = i
                    .IsHYPO = False
                    .IsPF = False
                    .VaporPressureEquation = 101
                    .IdealgasCpEquation = 5

                    'Add some surrogate standard parameters 
                    .Critical_Temperature = 2000
                    .Critical_Pressure = 30000000.0
                    .Critical_Volume = 0.554
                    .Acentric_Factor = 0.65
                    .Critical_Compressibility = 0.23
                    .Z_Rackett = 0.23
                    .LiquidDensityEquation = 1 'constant density
                    .Liquid_Density_Const_A = .SolidDensityAtTs / .Molar_Weight
                    .LiquidHeatCapacityEquation = 1 'constant Cp
                    .Liquid_Heat_Capacity_Const_A = .Electrolyte_Cp0 * 1000 * 1000 * 2 'this is an estimation from solid Cp due to missing values
                End With

                If CompName = "" Then
                    cpa.Add(cp)
                Else
                    If cp.Name = CompName Then
                        cpa.Add(cp)
                        Exit For
                    End If
                End If

                i += 1
            Next

            Return cpa.ToArray(Type.GetType("DWSIM.DWSIM.ClassesBasicasTermodinamica.ConstantProperties"))

        End Function

    End Class

    Public Class CoolProp

        Dim text() As String

        Sub New()

        End Sub

        Public Sub Load(ByVal filename As String)

            text = File.ReadAllLines(filename)

        End Sub

        Public Function Transfer() As ClassesBasicasTermodinamica.ConstantProperties()

            Dim cp As ClassesBasicasTermodinamica.ConstantProperties
            Dim cpa As New ArrayList()
            Dim i As Integer = 300000
            For Each s As String In text
                cp = New ClassesBasicasTermodinamica.ConstantProperties
                With cp
                    .OriginalDB = "CoolProp"
                    .Name = s ' & " [CoolProp]"
                    .Formula = ""
                    .CAS_Number = CProp.get_fluid_param_string(s, "CAS")
                    .Molar_Weight = CProp.Props1SI(s, "molemass") * 1000
                    .Critical_Pressure = CProp.Props1SI(s, "pcrit")
                    .Critical_Temperature = CProp.Props1SI(s, "Tcrit")
                    .Critical_Volume = 1 / CProp.Props1SI(s, "rhocrit") * .Molar_Weight
                    .Acentric_Factor = CProp.Props1SI(s, "acentric")
                    .Critical_Compressibility = .Critical_Pressure * .Critical_Volume / (8314 * .Critical_Temperature)
                    .ID = i
                    .IsHYPO = False
                    .IsPF = False
                    .IsCOOLPROPSupported = True
                End With
                cpa.Add(cp)
                i += 1
            Next

            Return cpa.ToArray(Type.GetType("DWSIM.DWSIM.ClassesBasicasTermodinamica.ConstantProperties"))

        End Function

    End Class

End Namespace
