using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace AgentWithTools.Tools;

public static class ReportTools
{
    [Description(
        "Gera uma planilha Excel formatada. " +
        "Permite definir título, colunas, linhas, cores, " +
        "tamanho da fonte, largura automática e filtros.")]
    public static string GenerateSalesExcel(
        [Description("Nome do arquivo, incluindo .xlsx")]
        string fileName,

        [Description("Título exibido acima da tabela")]
        string title,

        [Description("Nomes das colunas")]
        string[] columns,

        [Description("Dados: cada elemento representa uma linha")]
        string[][] rows,

        [Description("Cor de fundo do cabeçalho em hexadecimal, como #1F4E78")]
        string headerColor = "#1F4E78",

        [Description("Cor da fonte do cabeçalho em hexadecimal")]
        string headerFontColor = "#FFFFFF",

        [Description("Tamanho da fonte, entre 8 e 20")]
        int fontSize = 11,

        [Description("Ajustar largura das colunas ao conteúdo")]
        bool autoFitColumns = true,

        [Description("Congelar a linha do cabeçalho")]
        bool freezeHeader = true,

        [Description("Habilitar filtros nas colunas")]
        bool enableFilter = true)
    {
        if (columns.Length == 0)
            return "Erro: informe ao menos uma coluna.";

        if (rows.Any(row => row.Length != columns.Length))
            return "Erro: todas as linhas devem ter a mesma quantidade de valores das colunas.";

        fontSize = Math.Clamp(fontSize, 8, 20);

        // Limita os nomes a arquivos simples, sem caminhos arbitrários.
        fileName = Path.GetFileName(fileName);

        if (!fileName.EndsWith(".xlsx",
                StringComparison.OrdinalIgnoreCase))
            fileName += ".xlsx";

        string directory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile),
            "Downloads");

        Directory.CreateDirectory(directory);

        string filePath = Path.Combine(directory, fileName);

        // Valida as cores antes de gerar o documento.
        if (!System.Text.RegularExpressions.Regex.IsMatch(
                headerColor, "^#[0-9a-fA-F]{6}$") ||
            !System.Text.RegularExpressions.Regex.IsMatch(
                headerFontColor, "^#[0-9a-fA-F]{6}$"))
        {
            return "Erro: use cores hexadecimais no formato #RRGGBB.";
        }

        using XLWorkbook workbook = new XLWorkbook();

        IXLWorksheet worksheet = workbook.Worksheets.Add("Relatório");

        // Título.
        worksheet.Cell(1, 1).Value = title;
        worksheet.Range(1, 1, 1, columns.Length).Merge();

        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = fontSize + 4;
        worksheet.Cell(1, 1).Style.Font.FontColor =
            XLColor.FromHtml(headerColor);

        worksheet.Row(1).Height = 30;

        // Cabeçalho.
        for (int column = 0; column < columns.Length; column++)
        {
            IXLCell cell = worksheet.Cell(3, column + 1);
            cell.Value = columns[column];

            cell.Style.Fill.BackgroundColor =
                XLColor.FromHtml(headerColor);

            cell.Style.Font.FontColor =
                XLColor.FromHtml(headerFontColor);

            cell.Style.Font.Bold = true;
            cell.Style.Font.FontSize = fontSize;

            cell.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            cell.Style.Border.BottomBorder =
                XLBorderStyleValues.Thin;
        }

        // Dados.
        for (int row = 0; row < rows.Length; row++)
        {
            for (int column = 0;
                 column < columns.Length;
                 column++)
            {
                IXLCell cell = worksheet.Cell(row + 4, column + 1);
                cell.Value = rows[row][column];
                cell.Style.Font.FontSize = fontSize;

                if (row % 2 == 1)
                {
                    cell.Style.Fill.BackgroundColor =
                        XLColor.FromHtml("#F2F2F2");
                }
            }
        }

        int lastRow = rows.Length + 3;
        int lastColumn = columns.Length;

        if (enableFilter && rows.Length > 0)
        {
            worksheet.Range(3, 1, lastRow, lastColumn)
                .SetAutoFilter();
        }

        if (freezeHeader)
            worksheet.SheetView.FreezeRows(3);

        if (autoFitColumns)
            worksheet.Columns().AdjustToContents();

        workbook.SaveAs(filePath);

        return $"Excel gerado com sucesso: {filePath}";
    }

    [Description(
        "Gera um PDF com título, subtítulo, parágrafos, " +
        "tabela opcional, cores e tamanho de fonte personalizados.")]
    public static string GenerateSummaryPdf(
        [Description("Nome do arquivo PDF")]
        string fileName,

        [Description("Título do documento")]
        string title,

        [Description("Lista de parágrafos do documento")]
        string[] paragraphs,

        [Description("Cor do título em hexadecimal, como #1F4E78")]
        string titleColor = "#1F4E78",

        [Description("Cor de fundo do cabeçalho da tabela")]
        string headerColor = "#1F4E78",

        [Description("Tamanho da fonte do corpo, entre 8 e 18")]
        int fontSize = 11,

        [Description("Nomes das colunas da tabela, se houver")]
        string[]? columns = null,

        [Description("Linhas da tabela, se houver")]
        string[][]? rows = null)
    {
        fontSize = Math.Clamp(fontSize, 8, 18);

        fileName = Path.GetFileName(fileName);

        if (!fileName.EndsWith(
                ".pdf", StringComparison.OrdinalIgnoreCase))
            fileName += ".pdf";

        if (!Regex.IsMatch(titleColor, "^#[0-9a-fA-F]{6}$") ||
            !Regex.IsMatch(headerColor, "^#[0-9a-fA-F]{6}$"))
            return "Erro: use cores hexadecimais no formato #RRGGBB.";

        if (columns is { Length: > 0 } &&
            (rows ?? []).Any(row => row.Length != columns.Length))
            return "Erro: as linhas devem corresponder às colunas.";

        if (rows is { Length: > 0 } &&
            (columns is null || columns.Length == 0))
            return "Erro: informe as colunas da tabela.";

        string directory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile),
            "Downloads");

        Directory.CreateDirectory(directory);

        string filePath = Path.Combine(directory, fileName);

        Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.DefaultTextStyle(style =>
                    style.FontSize(fontSize));

                page.Header()
                    .Text(title)
                    .Bold()
                    .FontSize(fontSize + 8)
                    .FontColor(titleColor);

                page.Content()
                    .PaddingVertical(15)
                    .Column(content =>
                    {
                        content.Spacing(10);

                        foreach (string paragraph in paragraphs)
                        {
                            content.Item().Text(paragraph);
                        }

                        if (columns is { Length: > 0 })
                        {
                            content.Item().Table(table =>
                            {
                                table.ColumnsDefinition(definition =>
                                {
                                    foreach (string _ in columns)
                                        definition.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    foreach (string column in columns)
                                    {
                                        header.Cell()
                                            .Background(headerColor)
                                            .Padding(5)
                                            .Text(column)
                                            .Bold()
                                            .FontColor("#FFFFFF");
                                    }
                                });

                                foreach (string[] row in rows ?? [])
                                {
                                    foreach (string value in row)
                                    {
                                        table.Cell()
                                            .BorderBottom(1)
                                            .BorderColor("#DDDDDD")
                                            .Padding(5)
                                            .Text(value);
                                    }
                                }
                            });
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Gerado com Microsoft Foundry + .NET | Página ");
                        text.CurrentPageNumber();
                    });
            });
        }).GeneratePdf(filePath);

        return $"PDF gerado com sucesso: {filePath}";
    }
}