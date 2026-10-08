Imports Anegamiento

Public Class TerrenoTierra
    Inherits ComponenteTerreno

    Public Sub New(cuadrante As Integer)
        MyBase.New(cuadrante)
    End Sub

    Public Overrides ReadOnly Property PorcentajeAgua As Double
        Get
            Return 0
        End Get
    End Property

    Public Overrides ReadOnly Property PorcentajeTierra As Double
        Get
            Return 100
        End Get
    End Property



    Public Overrides Function AgregarHijo(componente As ComponenteTerreno) As Object
        Throw New NotImplementedException()
    End Function

    Public Overrides Function EliminarHijo(componente As ComponenteTerreno) As Object
        Throw New NotImplementedException()
    End Function

    Public Overrides Function ObtenerHijos() As List(Of ComponenteTerreno)
        Throw New NotImplementedException()
    End Function
End Class
