using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xceed.Words.NET; // Para manejar archivos .docx (necesita instalar la librería DocX desde NuGet)
using iText.Kernel.Pdf; // Para manejar archivos PDF (necesita instalar la librería iText7 desde NuGet)
using iText.Kernel.Pdf.Canvas.Parser;

namespace Archivos_No_Estructurados
{
   
    public partial class Form1 : Form
    {
        // Variable para recordar qué archivo abrimos
        string rutaArchivoActual = "";
        public Form1()
        {
            InitializeComponent();
            RtxtContenido.ReadOnly = true;
        }

        private void btnAbrir_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // Filtro universal para varios tipos de archivos no estructurados
            openFileDialog.Filter = "Textos planos (*.txt, *.log)|*.txt;*.log|Texto Enriquecido (*.rtf)|*.rtf|Documentos Word (*.docx)|*.docx|Archivos PDF (*.pdf)|*.pdf"; openFileDialog.Title = "Abrir archivo no estructurado";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                rutaArchivoActual = openFileDialog.FileName;

                try
                {
                    // Obtenemos la extensión del archivo (ej. ".txt", ".rtf")
                    string extension = Path.GetExtension(rutaArchivoActual).ToLower();

                    switch (extension)
                    {
                        case ".txt": lblTipodeArchivo.Text = "Tipo de archivo: Texto plano (.txt)"; break;
                        case ".log": lblTipodeArchivo.Text = "Tipo de archivo: Bitácora (.log)"; break;
                        case ".rtf": lblTipodeArchivo.Text = "Tipo de archivo: Texto enriquecido (.rtf)"; break;
                        case ".docx": lblTipodeArchivo.Text = "Tipo de archivo: Documento Word (.docx)"; break;
                        case ".pdf": lblTipodeArchivo.Text = "Tipo de archivo: Documento PDF (.pdf)"; break;
                        default: lblTipodeArchivo.Text = "Tipo de archivo: " + extension.ToUpper(); break;
                    }

                    // Decidimos cómo abrirlo dependiendo de su tipo
                    switch (extension)
                    {
                        case ".txt":
                        case ".log":
                        case ".md":
                            // Lectura de texto crudo
                            RtxtContenido.Text = File.ReadAllText(rutaArchivoActual);
                            break;

                        case ".rtf":
                            // Lectura nativa de texto con formato (colores, fuentes)
                            RtxtContenido.LoadFile(rutaArchivoActual, RichTextBoxStreamType.RichText);
                            break;

                        case ".docx":
                            // 1. Cargamos el documento de Word usando la librería
                            using (DocX documento = DocX.Load(rutaArchivoActual))
                            {
                                // 2. Extraemos todo el texto limpio y lo mostramos en pantalla
                                RtxtContenido.Text = documento.Text;
                            }
                            break;

                        case ".pdf":
                            // 1. Preparamos un "armador de textos" (StringBuilder es excelente para PDFs de muchas páginas)
                            StringBuilder textoPDF = new StringBuilder();

                            // 2. Abrimos el documento PDF usando la librería iText7
                            using (PdfDocument pdfDocumento = new PdfDocument(new PdfReader(rutaArchivoActual)))
                            {
                                // 3. Hacemos un ciclo para recorrer el PDF página por página
                                for (int i = 1; i <= pdfDocumento.GetNumberOfPages(); i++)
                                {
                                    // Extraemos el texto de la página actual y le agregamos un salto de línea al final
                                    textoPDF.AppendLine(PdfTextExtractor.GetTextFromPage(pdfDocumento.GetPage(i)));
                                }
                            }

                            // 4. Mostramos todo el texto extraído en tu RichTextBox
                            RtxtContenido.Text = textoPDF.ToString();
                            break;

                        default:
                            MessageBox.Show("El formato seleccionado se abrirá como texto plano, pero podría contener caracteres ilegibles.", "Formato Desconocido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            RtxtContenido.Text = File.ReadAllText(rutaArchivoActual);
                            break;
                    }

                    // Bloqueamos la edición hasta que se presione "Modificar"
                    RtxtContenido.ReadOnly = true;
                    btnModificar.Text = "Modificar";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al leer el archivo:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCrear_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivo de Texto (*.txt)|*.txt|Texto Enriquecido (*.rtf)|*.rtf";
            saveFileDialog.Title = "Crear nuevo archivo";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                rutaArchivoActual = saveFileDialog.FileName;
                string extension = Path.GetExtension(rutaArchivoActual).ToLower();
                if (extension == ".rtf")
                {
                    lblTipodeArchivo.Text = "Tipo de archivo: Texto enriquecido (.rtf)";
                }
                else
                {
                    lblTipodeArchivo.Text = "Tipo de archivo: Texto plano (.txt)";
                }

                // Limpiamos la pantalla
                RtxtContenido.Clear();

                try
                {
                    // Creamos el archivo vacío según el formato elegido
                    if (extension == ".rtf")
                    {
                        RtxtContenido.SaveFile(rutaArchivoActual, RichTextBoxStreamType.RichText);
                    }
                    else
                    {
                        File.WriteAllText(rutaArchivoActual, "");
                    }

                    // Habilitamos la escritura automáticamente
                    RtxtContenido.ReadOnly = false;
                    btnModificar.Text = "Bloquear Edición";

                    MessageBox.Show("Archivo creado. Ya puedes empezar a escribir.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al crear el archivo:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(rutaArchivoActual))
            {
                MessageBox.Show("Primero abre o crea un archivo.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Si está bloqueado, lo abrimos para editar
            if (RtxtContenido.ReadOnly == true)
            {
                RtxtContenido.ReadOnly = false;
                btnModificar.Text = "Bloquear Edición";
                RtxtContenido.Focus(); // Pone el cursor parpadeando adentro
            }
            // Si ya estaba editable, lo bloqueamos de nuevo
            else
            {
                RtxtContenido.ReadOnly = true;
                btnModificar.Text = "Modificar";
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(rutaArchivoActual))
            {
                MessageBox.Show("No hay ningún archivo abierto para guardar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string extension = Path.GetExtension(rutaArchivoActual).ToLower();

                // Guardamos el archivo dependiendo de su formato
                switch (extension)
                {
                    case ".txt":
                    case ".log":
                    case ".md":
                        File.WriteAllText(rutaArchivoActual, RtxtContenido.Text);
                        break;

                    case ".rtf":
                        // Guarda el texto conservando colores, fuentes y negritas
                        RtxtContenido.SaveFile(rutaArchivoActual, RichTextBoxStreamType.RichText);
                        break;

                    case ".docx":
                    case ".pdf":
                        MessageBox.Show("Aún no tienes instaladas las librerías para sobrescribir este tipo de documentos.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return; // Salimos para no intentar sobrescribir con texto plano

                    default:
                        File.WriteAllText(rutaArchivoActual, RtxtContenido.Text);
                        break;
                }

                // Bloqueamos por seguridad para indicar que ya se guardó
                RtxtContenido.ReadOnly = true;
                btnModificar.Text = "Modificar";

                MessageBox.Show("Tu archivo se ha guardado exitosamente.", "Guardado Exitoso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al intentar guardar:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEliminarArchivo_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(rutaArchivoActual))
            {
                MessageBox.Show("No hay ningún archivo abierto para eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirmacion = MessageBox.Show($"¿Estás seguro de eliminar permanentemente el archivo:\n{rutaArchivoActual}?", "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmacion == DialogResult.Yes)
            {
                try
                {
                    File.Delete(rutaArchivoActual);

                    // Limpiamos la pantalla y variables
                    rutaArchivoActual = "";
                    RtxtContenido.Clear();
                    RtxtContenido.ReadOnly = true;
                    btnModificar.Text = "Modificar";

                    MessageBox.Show("El archivo ha sido eliminado correctamente de tu computadora.", "Archivo Eliminado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al eliminar el archivo:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
