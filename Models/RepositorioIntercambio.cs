using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MiProyecto.Models
{
    public class RepositorioIntercambio : RepositorioBase, IRepositorioIntercambio
    {
        public RepositorioIntercambio(IConfiguration configuration) : base(configuration) { }

        public int Alta(Intercambio e)
        {
            throw new NotImplementedException(" CrearIntercambio en lugar de Alta");
        }

        public int Baja(int id)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var query = "DELETE FROM intercambio WHERE IdIntercambio = @id";
                using (var command = new MySqlCommand(query, (MySqlConnection)connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    return command.ExecuteNonQuery();
                }
            }
        }

        public int Modificacion(Intercambio e)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var query = @"UPDATE intercambio SET 
                            IdUsuarioEmisor = @IdUsuarioEmisor,
                            IdUsuarioReceptor = @IdUsuarioReceptor,
                            IdColeccionEmisor = @IdColeccionEmisor,
                            IdColeccionReceptor = @IdColeccionReceptor,
                            Fecha = @Fecha,
                            Estado = @Estado
                            WHERE IdIntercambio = @IdIntercambio";

                using (var command = new MySqlCommand(query, (MySqlConnection)connection))
                {
                    command.Parameters.AddWithValue("@IdIntercambio", e.IdIntercambio);
                    command.Parameters.AddWithValue("@IdUsuarioEmisor", e.IdUsuarioEmisor);
                    command.Parameters.AddWithValue("@IdUsuarioReceptor", e.IdUsuarioReceptor);
                    command.Parameters.AddWithValue("@IdColeccionEmisor", e.IdColeccionEmisor);
                    command.Parameters.AddWithValue("@IdColeccionReceptor", e.IdColeccionReceptor);
                    command.Parameters.AddWithValue("@Fecha", e.Fecha);
                    command.Parameters.AddWithValue("@Estado", e.Estado);

                    return command.ExecuteNonQuery();
                }
            }
        }

      public Intercambio ObtenerPorId(int id)
        {
            Intercambio intercambio = null;
            using (var connection = GetConnection())
            {
                try
                {
                    
                    connection.Open();
                    
                    var query = @"SELECT i.*, 
                                ue.Nombre as EmisorNombre, ue.Apellido as EmisorApellido, ue.Avatar as EmisorAvatar,
                                ur.Nombre as ReceptorNombre, ur.Apellido as ReceptorApellido, ur.Avatar as ReceptorAvatar,
                                ce.Nombre as ColeccionEmisorNombre, cr.Nombre as ColeccionReceptorNombre
                                FROM intercambio i
                                JOIN usuario ue ON i.IdUsuarioEmisor = ue.IdUsuario
                                JOIN usuario ur ON i.IdUsuarioReceptor = ur.IdUsuario
                                JOIN coleccion ce ON i.IdColeccionEmisor = ce.IdColeccion
                                JOIN coleccion cr ON i.IdColeccionReceptor = cr.IdColeccion
                                WHERE i.IdIntercambio = @id";

                    using (var command = new MySqlCommand(query, (MySqlConnection)connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        
                        using (var reader = command.ExecuteReader())
                        {
                            
                            if (reader.Read())
                            {
                                intercambio = new Intercambio
                                {
                                    IdIntercambio = reader.GetInt32("IdIntercambio"),
                                    IdUsuarioEmisor = reader.GetInt32("IdUsuarioEmisor"),
                                    IdUsuarioReceptor = reader.GetInt32("IdUsuarioReceptor"),
                                    IdColeccionEmisor = reader.GetInt32("IdColeccionEmisor"),
                                    IdColeccionReceptor = reader.GetInt32("IdColeccionReceptor"),
                                    Fecha = reader.GetDateTime("Fecha"),
                                    Estado = reader.GetInt32("Estado"),
                                    Emisor = new Usuario
                                    {
                                        Nombre = reader.GetString("EmisorNombre"),
                                        Apellido = reader.GetString("EmisorApellido"),
                                        Avatar = reader.GetString("EmisorAvatar")
                                    },
                                    Receptor = new Usuario
                                    {
                                        Nombre = reader.GetString("ReceptorNombre"),
                                        Apellido = reader.GetString("ReceptorApellido"),
                                        Avatar = reader.GetString("ReceptorAvatar")
                                    },
                                    ColeccionEmisor = new Coleccion
                                    {
                                        Nombre = reader.GetString("ColeccionEmisorNombre")
                                    },
                                    ColeccionReceptor = new Coleccion
                                    {
                                        Nombre = reader.GetString("ColeccionReceptorNombre")
                                    },
                                    Cartas = new List<IntercambioCarta>()
                                };
                                
                            }
                            else
                            {
                                
                                return null;
                            }
                        }
                    }

                    if (intercambio != null)
                    {
                        
                        try
                        {
                            intercambio.Cartas = ObtenerCartasIntercambio(intercambio.IdIntercambio);
                            
                        }
                        catch (Exception ex)
                        {
                            
                            throw;
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    
                    throw;
                }
                catch (Exception ex)
                {
                    
                    throw;
                }
                finally
                {
                    Console.WriteLine($"[ObtenerPorId] Completado id={id}");
                }
            }
            return intercambio;
        }
        public IList<Intercambio> ObtenerTodos(int pagina, int tamPagina = 10)
        {
            var intercambios = new List<Intercambio>();
            using (var connection = GetConnection())
            {
                connection.Open();
                var query = @"SELECT i.*, 
                            ue.Nombre as EmisorNombre, ue.Apellido as EmisorApellido, ue.Avatar as EmisorAvatar,
                            ur.Nombre as ReceptorNombre, ur.Apellido as ReceptorApellido, ur.Avatar as ReceptorAvatar,
                            ce.Nombre as ColeccionEmisorNombre, cr.Nombre as ColeccionReceptorNombre
                            FROM intercambio i
                            JOIN usuario ue ON i.IdUsuarioEmisor = ue.IdUsuario
                            JOIN usuario ur ON i.IdUsuarioReceptor = ur.IdUsuario
                            JOIN coleccion ce ON i.IdColeccionEmisor = ce.IdColeccion
                            JOIN coleccion cr ON i.IdColeccionReceptor = cr.IdColeccion
                            ORDER BY i.Fecha DESC
                            LIMIT @tamPagina OFFSET @offset";

                using (var command = new MySqlCommand(query, (MySqlConnection)connection))
                {
                    command.Parameters.AddWithValue("@tamPagina", tamPagina);
                    command.Parameters.AddWithValue("@offset", (pagina - 1) * tamPagina);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var intercambio = new Intercambio
                            {
                                IdIntercambio = reader.GetInt32("IdIntercambio"),
                                IdUsuarioEmisor = reader.GetInt32("IdUsuarioEmisor"),
                                IdUsuarioReceptor = reader.GetInt32("IdUsuarioReceptor"),
                                IdColeccionEmisor = reader.GetInt32("IdColeccionEmisor"),
                                IdColeccionReceptor = reader.GetInt32("IdColeccionReceptor"),
                                Fecha = reader.GetDateTime("Fecha"),
                                Estado = reader.GetInt32("Estado"),
                                Emisor = new Usuario
                                {
                                    Nombre = reader.GetString("EmisorNombre"),
                                    Apellido = reader.GetString("EmisorApellido"),
                                    Avatar = reader.GetString("EmisorAvatar")
                                },
                                Receptor = new Usuario
                                {
                                    Nombre = reader.GetString("ReceptorNombre"),
                                    Apellido = reader.GetString("ReceptorApellido"),
                                    Avatar = reader.GetString("ReceptorAvatar")
                                },
                                ColeccionEmisor = new Coleccion
                                {
                                    Nombre = reader.GetString("ColeccionEmisorNombre")
                                },
                                ColeccionReceptor = new Coleccion
                                {
                                    Nombre = reader.GetString("ColeccionReceptorNombre")
                                }
                            };
                            intercambios.Add(intercambio);
                        }
                    }
                }
            }
            return intercambios;
        }

        public int CrearIntercambio(Intercambio intercambio, List<IntercambioCarta> cartasEmisor, List<IntercambioCarta> cartasReceptor)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        
                        var query = @"INSERT INTO intercambio 
                                    (IdUsuarioEmisor, IdUsuarioReceptor, IdColeccionEmisor, IdColeccionReceptor, Fecha, Estado)
                                    VALUES (@IdUsuarioEmisor, @IdUsuarioReceptor, @IdColeccionEmisor, @IdColeccionReceptor, @Fecha, @Estado);
                                    SELECT LAST_INSERT_ID();";

                        int idIntercambio;
                        using (var command = new MySqlCommand(query, (MySqlConnection)connection, (MySqlTransaction)transaction))
                        {
                            command.Parameters.AddWithValue("@IdUsuarioEmisor", intercambio.IdUsuarioEmisor);
                            command.Parameters.AddWithValue("@IdUsuarioReceptor", intercambio.IdUsuarioReceptor);
                            command.Parameters.AddWithValue("@IdColeccionEmisor", intercambio.IdColeccionEmisor);
                            command.Parameters.AddWithValue("@IdColeccionReceptor", intercambio.IdColeccionReceptor);
                            command.Parameters.AddWithValue("@Fecha", DateTime.Now);
                            command.Parameters.AddWithValue("@Estado", 0);
                            
                            idIntercambio = Convert.ToInt32(command.ExecuteScalar());
                            
                        }

                        if (idIntercambio <= 0)
                        {
                            
                            throw new Exception("Fallo al insertar IdIntercambio");
                        }

                        
                        foreach (var carta in cartasEmisor)
                        {
                            if (carta.IdCarta <= 0 || carta.Cantidad <= 0)
                            {
                                
                                throw new Exception("Invalida carta emisor data");
                            }
                            var queryCarta = @"INSERT INTO intercambio_carta 
                                            (IdIntercambio, IdCarta, Cantidad, EsDeEmisor)
                                            VALUES (@IdIntercambio, @IdCarta, @Cantidad, 1)";
                            using (var command = new MySqlCommand(queryCarta, (MySqlConnection)connection, (MySqlTransaction)transaction))
                            {
                                command.Parameters.AddWithValue("@IdIntercambio", idIntercambio);
                                command.Parameters.AddWithValue("@IdCarta", carta.IdCarta);
                                command.Parameters.AddWithValue("@Cantidad", carta.Cantidad);
                                
                                command.ExecuteNonQuery();
                                
                            }
                        }

                        
                        foreach (var carta in cartasReceptor)
                        {
                            if (carta.IdCarta <= 0 || carta.Cantidad <= 0)
                            {
                                
                                throw new Exception("Invalida carta receptor data");
                            }
                            var queryCarta = @"INSERT INTO intercambio_carta 
                                            (IdIntercambio, IdCarta, Cantidad, EsDeEmisor)
                                            VALUES (@IdIntercambio, @IdCarta, @Cantidad, 0)";
                            using (var command = new MySqlCommand(queryCarta, (MySqlConnection)connection, (MySqlTransaction)transaction))
                            {
                                command.Parameters.AddWithValue("@IdIntercambio", idIntercambio);
                                command.Parameters.AddWithValue("@IdCarta", carta.IdCarta);
                                command.Parameters.AddWithValue("@Cantidad", carta.Cantidad);
                                
                                command.ExecuteNonQuery();
                                
                            }
                        }

                        
                        var queryUpdateColeccion = @"UPDATE coleccion SET EsPublica = 0 
                                                    WHERE IdColeccion IN (@IdColeccionEmisor, @IdColeccionReceptor)";
                        using (var command = new MySqlCommand(queryUpdateColeccion, (MySqlConnection)connection, (MySqlTransaction)transaction))
                        {
                            command.Parameters.AddWithValue("@IdColeccionEmisor", intercambio.IdColeccionEmisor);
                            command.Parameters.AddWithValue("@IdColeccionReceptor", intercambio.IdColeccionReceptor);
                            
                            command.ExecuteNonQuery();
                            
                        }

                        transaction.Commit();
                        
                        return idIntercambio;
                    }
                    catch (MySqlException ex) when (ex.Number == 1062)
                    {
                        
                        transaction.Rollback();
                        using (var command = new MySqlCommand("SELECT COUNT(*) FROM intercambio; SELECT COUNT(*) FROM intercambio_carta;", (MySqlConnection)connection))
                        {
                            using (var reader = command.ExecuteReader())
                            {
                                reader.Read();
                                Console.WriteLine($"intercambio tabla filas contador: {reader.GetInt32(0)}");
                                reader.NextResult();
                                reader.Read();
                                Console.WriteLine($"intercambio_carta tabla filas contador: {reader.GetInt32(0)}");
                            }
                        }
                        throw new Exception($"Error al crear el intercambio: Entrada duplicada - {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        
                        transaction.Rollback();
                        using (var command = new MySqlCommand("SELECT COUNT(*) FROM intercambio; SELECT COUNT(*) FROM intercambio_carta;", (MySqlConnection)connection))
                        {
                            using (var reader = command.ExecuteReader())
                            {
                                reader.Read();
                                Console.WriteLine($"intercambio tabla filas contador: {reader.GetInt32(0)}");
                                reader.NextResult();
                                reader.Read();
                                Console.WriteLine($"intercambio_carta tabla filas contador: {reader.GetInt32(0)}");
                            }
                        }
                        throw;
                    }
                }
            }
        }

        public bool TieneIntercambioPendiente(int idUsuario)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var query = @"SELECT COUNT(*) FROM intercambio 
                            WHERE (IdUsuarioEmisor = @idUsuario OR IdUsuarioReceptor = @idUsuario)
                            AND Estado = 0"; // 0 = Pendiente

                using (var command = new MySqlCommand(query, (MySqlConnection)connection))
                {
                    command.Parameters.AddWithValue("@idUsuario", idUsuario);
                    var count = Convert.ToInt32(command.ExecuteScalar());
                    
                    return count > 0;
                }
            }
        }

        public bool AceptarIntercambio(int idIntercambio)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Obtener el intercambio
                        var intercambio = ObtenerPorId(idIntercambio);
                        if (intercambio == null || intercambio.Estado != 0) // 0 = Pendiente
                            return false;

                        // 1. Eliminar cartas de las colecciones originales
                        // Cartas del emisor (van al receptor)
                        var cartasEmisor = intercambio.Cartas.Where(c => c.EsDeEmisor == 1).ToList();
                        foreach (var carta in cartasEmisor)
                        {
                            // Restar de colección emisor
                            var queryRestarEmisor = @"UPDATE coleccion_carta 
                                                     SET Cantidad = Cantidad - @Cantidad
                                                     WHERE IdColeccion = @IdColeccionEmisor AND IdCarta = @IdCarta";
                            using (var command = new MySqlCommand(queryRestarEmisor, (MySqlConnection)connection, (MySqlTransaction)transaction))
                            {
                                command.Parameters.AddWithValue("@IdColeccionEmisor", intercambio.IdColeccionEmisor);
                                command.Parameters.AddWithValue("@IdCarta", carta.IdCarta);
                                command.Parameters.AddWithValue("@Cantidad", carta.Cantidad);
                                command.ExecuteNonQuery();
                            }

                            // Agregar a coleccion receptor
                            var queryAgregarReceptor = @"INSERT INTO coleccion_carta 
                                                        (IdColeccion, IdCarta, Cantidad)
                                                        VALUES (@IdColeccionReceptor, @IdCarta, @Cantidad)
                                                        ON DUPLICATE KEY UPDATE Cantidad = Cantidad + @Cantidad";
                            using (var command = new MySqlCommand(queryAgregarReceptor, (MySqlConnection)connection, (MySqlTransaction)transaction))
                            {
                                command.Parameters.AddWithValue("@IdColeccionReceptor", intercambio.IdColeccionReceptor);
                                command.Parameters.AddWithValue("@IdCarta", carta.IdCarta);
                                command.Parameters.AddWithValue("@Cantidad", carta.Cantidad);
                                command.ExecuteNonQuery();
                            }
                        }

                        // Cartas del receptor (van al emisor)
                        var cartasReceptor = intercambio.Cartas.Where(c => c.EsDeEmisor == 0).ToList();
                        foreach (var carta in cartasReceptor)
                        {
                            // Restar de coleccion receptor
                            var queryRestarReceptor = @"UPDATE coleccion_carta 
                                                       SET Cantidad = Cantidad - @Cantidad
                                                       WHERE IdColeccion = @IdColeccionReceptor AND IdCarta = @IdCarta";
                            using (var command = new MySqlCommand(queryRestarReceptor, (MySqlConnection)connection, (MySqlTransaction)transaction))
                            {
                                command.Parameters.AddWithValue("@IdColeccionReceptor", intercambio.IdColeccionReceptor);
                                command.Parameters.AddWithValue("@IdCarta", carta.IdCarta);
                                command.Parameters.AddWithValue("@Cantidad", carta.Cantidad);
                                command.ExecuteNonQuery();
                            }

                            // Agregar a colección emisor
                            var queryAgregarEmisor = @"INSERT INTO coleccion_carta 
                                                      (IdColeccion, IdCarta, Cantidad)
                                                      VALUES (@IdColeccionEmisor, @IdCarta, @Cantidad)
                                                      ON DUPLICATE KEY UPDATE Cantidad = Cantidad + @Cantidad";
                            using (var command = new MySqlCommand(queryAgregarEmisor, (MySqlConnection)connection, (MySqlTransaction)transaction))
                            {
                                command.Parameters.AddWithValue("@IdColeccionEmisor", intercambio.IdColeccionEmisor);
                                command.Parameters.AddWithValue("@IdCarta", carta.IdCarta);
                                command.Parameters.AddWithValue("@Cantidad", carta.Cantidad);
                                command.ExecuteNonQuery();
                            }
                        }

                        // 2. Actualizar estado del intercambio a Aceptado (1)
                        var queryUpdate = @"UPDATE intercambio SET Estado = 1 WHERE IdIntercambio = @IdIntercambio";
                        using (var command = new MySqlCommand(queryUpdate, (MySqlConnection)connection, (MySqlTransaction)transaction))
                        {
                            command.Parameters.AddWithValue("@IdIntercambio", idIntercambio);
                            command.ExecuteNonQuery();
                        }

                        // 3. Volver a hacer publicas las colecciones
                        var queryPublicar = @"UPDATE coleccion SET EsPublica = 1 
                                           WHERE IdColeccion IN (@IdColeccionEmisor, @IdColeccionReceptor)";
                        using (var command = new MySqlCommand(queryPublicar, (MySqlConnection)connection, (MySqlTransaction)transaction))
                        {
                            command.Parameters.AddWithValue("@IdColeccionEmisor", intercambio.IdColeccionEmisor);
                            command.Parameters.AddWithValue("@IdColeccionReceptor", intercambio.IdColeccionReceptor);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public bool CancelarIntercambio(int idIntercambio)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Obtener el intercambio
                        var intercambio = ObtenerPorId(idIntercambio);
                        if (intercambio == null || intercambio.Estado != 0) // 0 = Pendiente
                            return false;

                        // 1. Actualizar estado del intercambio a Cancelado (2)
                        var queryUpdate = @"UPDATE intercambio SET Estado = 2 WHERE IdIntercambio = @IdIntercambio";
                        using (var command = new MySqlCommand(queryUpdate, (MySqlConnection)connection, (MySqlTransaction)transaction))
                        {
                            command.Parameters.AddWithValue("@IdIntercambio", idIntercambio);
                            command.ExecuteNonQuery();
                        }

                        // 2. Volver a hacer publicas las colecciones
                        var queryPublicar = @"UPDATE coleccion SET EsPublica = 1 
                                           WHERE IdColeccion IN (@IdColeccionEmisor, @IdColeccionReceptor)";
                        using (var command = new MySqlCommand(queryPublicar, (MySqlConnection)connection, (MySqlTransaction)transaction))
                        {
                            command.Parameters.AddWithValue("@IdColeccionEmisor", intercambio.IdColeccionEmisor);
                            command.Parameters.AddWithValue("@IdColeccionReceptor", intercambio.IdColeccionReceptor);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

       public IList<Intercambio> ObtenerPorUsuario(int idUsuario)
        {
            var intercambios = new List<Intercambio>();
            using (var connection = GetConnection())
            {
                try
                {
                    
                    connection.Open();
                    
                    var query = @"SELECT i.*, 
                                ue.Nombre as EmisorNombre, ue.Apellido as EmisorApellido, ue.Avatar as EmisorAvatar,
                                ur.Nombre as ReceptorNombre, ur.Apellido as ReceptorApellido, ur.Avatar as ReceptorAvatar,
                                ce.Nombre as ColeccionEmisorNombre, cr.Nombre as ColeccionReceptorNombre
                                FROM intercambio i
                                JOIN usuario ue ON i.IdUsuarioEmisor = ue.IdUsuario
                                JOIN usuario ur ON i.IdUsuarioReceptor = ur.IdUsuario
                                JOIN coleccion ce ON i.IdColeccionEmisor = ce.IdColeccion
                                JOIN coleccion cr ON i.IdColeccionReceptor = cr.IdColeccion
                                WHERE i.IdUsuarioEmisor = @idUsuario OR i.IdUsuarioReceptor = @idUsuario
                                ORDER BY i.Fecha DESC";

                    using (var command = new MySqlCommand(query, (MySqlConnection)connection))
                    {
                        command.Parameters.AddWithValue("@idUsuario", idUsuario);
                        
                        using (var reader = command.ExecuteReader())
                        {
                           
                            while (reader.Read())
                            {
                                var intercambio = new Intercambio
                                {
                                    IdIntercambio = reader.GetInt32("IdIntercambio"),
                                    IdUsuarioEmisor = reader.GetInt32("IdUsuarioEmisor"),
                                    IdUsuarioReceptor = reader.GetInt32("IdUsuarioReceptor"),
                                    IdColeccionEmisor = reader.GetInt32("IdColeccionEmisor"),
                                    IdColeccionReceptor = reader.GetInt32("IdColeccionReceptor"),
                                    Fecha = reader.GetDateTime("Fecha"),
                                    Estado = reader.GetInt32("Estado"),
                                    Emisor = new Usuario
                                    {
                                        Nombre = reader.GetString("EmisorNombre"),
                                        Apellido = reader.GetString("EmisorApellido"),
                                        Avatar = reader.GetString("EmisorAvatar")
                                    },
                                    Receptor = new Usuario
                                    {
                                        Nombre = reader.GetString("ReceptorNombre"),
                                        Apellido = reader.GetString("ReceptorApellido"),
                                        Avatar = reader.GetString("ReceptorAvatar")
                                    },
                                    ColeccionEmisor = new Coleccion
                                    {
                                        Nombre = reader.GetString("ColeccionEmisorNombre")
                                    },
                                    ColeccionReceptor = new Coleccion
                                    {
                                        Nombre = reader.GetString("ColeccionReceptorNombre")
                                    },
                                    Cartas = new List<IntercambioCarta>()
                                };
                                
                                intercambios.Add(intercambio);
                            }
                        }
                    }

                    // obtener las cartas de cada intercambio
                    foreach (var intercambio in intercambios)
                    {
                        
                        try
                        {
                            intercambio.Cartas = ObtenerCartasIntercambio(intercambio.IdIntercambio);
                            
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ObtenerPorUsuario] Error en ObtenerCartasIntercambio: {ex.Message}, IdIntercambio={intercambio.IdIntercambio}");
                            throw;
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"[ObtenerPorUsuario] MySQL Error: {ex.Message}, Code: {ex.Number}, idUsuario={idUsuario}");
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ObtenerPorUsuario] General Error: {ex.Message}, idUsuario={idUsuario}");
                    throw;
                }
                finally
                {
                    Console.WriteLine($"[ObtenerPorUsuario] Completed for idUsuario={idUsuario}");
                }
            }
            return intercambios;
        }

      public List<IntercambioCarta> ObtenerCartasIntercambio(int idIntercambio)
        {
            var cartas = new List<IntercambioCarta>();
            using (var connection = GetConnection())
            {
                try
                {
                    
                    connection.Open();
                    
                    var query = @"SELECT ic.IdIntercambio, ic.IdCarta, ic.Cantidad, ic.EsDeEmisor,
                                c.Nombre, c.Imagen, c.ValorEstimado
                                FROM intercambio_carta ic
                                LEFT JOIN carta c ON ic.IdCarta = c.IdCarta
                                WHERE ic.IdIntercambio = @IdIntercambio";

                    using (var command = new MySqlCommand(query, (MySqlConnection)connection))
                    {
                        command.Parameters.AddWithValue("@IdIntercambio", idIntercambio);
                       
                        using (var reader = command.ExecuteReader())
                        {
                            
                            
                            while (reader.Read())
                            {
                                
                                var carta = new IntercambioCarta
                                {
                                    IdIntercambio = reader.GetInt32("IdIntercambio"),
                                    IdCarta = reader.GetInt32("IdCarta"),
                                    Cantidad = reader.GetInt32("Cantidad"),
                                    EsDeEmisor = reader.GetInt32("EsDeEmisor"),
                                    Carta = new Carta
                                    {
                                        Nombre = reader.IsDBNull(reader.GetOrdinal("Nombre")) ? "Unknown" : reader.GetString("Nombre"),
                                        Imagen = reader.IsDBNull(reader.GetOrdinal("Imagen")) ? null : reader.GetString("Imagen"),
                                        ValorEstimado = reader.IsDBNull(reader.GetOrdinal("ValorEstimado")) ? 0 : reader.GetInt32("ValorEstimado")
                                    }
                                };
                                
                                cartas.Add(carta);
                            }
                            
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"[ObtenerCartasIntercambio] MySQL Error: {ex.Message}, Code: {ex.Number}, IdIntercambio={idIntercambio}");
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ObtenerCartasIntercambio] General Error: {ex.Message}, IdIntercambio={idIntercambio}");
                    throw;
                }
            }
            return cartas;
        }
        public int ContarTodos()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var query = "SELECT COUNT(*) FROM intercambio";
                using (var command = new MySqlCommand(query, (MySqlConnection)connection))
                {
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
       
    }
}