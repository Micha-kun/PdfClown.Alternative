namespace org.pdfclown.samples.cli
{
    using System.Drawing;
    using org.pdfclown.documents;
    using org.pdfclown.documents.contents.composition;
    using org.pdfclown.documents.contents.fonts;
    using org.pdfclown.documents.interaction.actions;
    using org.pdfclown.documents.interaction.annotations;
    using org.pdfclown.documents.interaction.forms;

    using org.pdfclown.documents.interaction.forms.styles;
    using org.pdfclown.files;

    /**
      <summary>This sample demonstrates how to insert AcroForm fields into a PDF document.</summary>
    */
    public class AcroFormCreationSample
      : Sample
    {

        private void Populate(
          Document document
          )
        {
            /*
              NOTE: In order to insert a field into a document, you have to follow these steps:
              1. Define the form fields collection that will gather your fields (NOTE: the form field collection is global to the document);
              2. Define the pages where to place the fields;
              3. Define the appearance style to render your fields;
              4. Create each field of yours:
                4.1. instantiate your field into the page;
                4.2. apply the appearance style to your field;
                4.3. insert your field into the fields collection.
            */

            // 1. Define the form fields collection!
            var form = document.Form;
            var fields = form.Fields;

            // 2. Define the page where to place the fields!
            var page = new Page(document);
            document.Pages.Add(page);

            // 3. Define the appearance style to apply to the fields!
            var fieldStyle = new DefaultStyle();
            fieldStyle.FontSize = 12;
            fieldStyle.GraphicsVisibile = true;

            var composer = new PrimitiveComposer(page);
            composer.SetFont(
              new StandardType1Font(
                document,
                StandardType1Font.FamilyEnum.Courier,
                true,
                false
                ),
              14
              );

            // 4. Field creation.
            // 4.a. Push button.
            {
                _ = composer.ShowText(
                  "PushButton:",
                  new PointF(140, 68),
                  XAlignmentEnum.Right,
                  YAlignmentEnum.Middle,
                  0
                  );

                var fieldWidget = new Widget(
                  page,
                  new RectangleF(150, 50, 136, 36)
                  );
                fieldWidget.Actions.OnActivate = new JavaScript(
                  document,
                  "app.alert(\"Radio button currently selected: '\" + this.getField(\"myRadio\").value + \"'.\",3,0,\"Activation event\");"
                  );
                var field = new PushButton(
                  "okButton",
                  fieldWidget,
                  "Push" // Current value.
                  ); // 4.1. Field instantiation.
                fields.Add(field); // 4.2. Field insertion into the fields collection.
                fieldStyle.Apply(field); // 4.3. Appearance style applied.
                var blockComposer = new BlockComposer(composer);
                blockComposer.Begin(new RectangleF(296, 50, page.Size.Width - 336, 36), XAlignmentEnum.Left, YAlignmentEnum.Middle);
                composer.SetFont(composer.State.Font, 7);
                _ = blockComposer.ShowText("If you click this push button, a javascript action should prompt you an alert box responding to the activation event triggered by your PDF viewer.");
                blockComposer.End();
            }

            // 4.b. Check box.
            {
                _ = composer.ShowText(
                  "CheckBox:",
                  new PointF(140, 118),
                  XAlignmentEnum.Right,
                  YAlignmentEnum.Middle,
                  0
                  );
                var field = new CheckBox(
                  "myCheck",
                  new Widget(
                    page,
                    new RectangleF(150, 100, 36, 36)
                    ),
                  true // Current value.
                  ); // 4.1. Field instantiation.
                fieldStyle.Apply(field);
                fields.Add(field);
                field = new CheckBox(
                  "myCheck2",
                  new Widget(
                    page,
                    new RectangleF(200, 100, 36, 36)
                    ),
                  true // Current value.
                  ); // 4.1. Field instantiation.
                fieldStyle.Apply(field);
                fields.Add(field);
                field = new CheckBox(
                  "myCheck3",
                  new Widget(
                    page,
                    new RectangleF(250, 100, 36, 36)
                    ),
                  false // Current value.
                  ); // 4.1. Field instantiation.
                fields.Add(field); // 4.2. Field insertion into the fields collection.
                fieldStyle.Apply(field); // 4.3. Appearance style applied.
            }

            // 4.c. Radio button.
            {
                _ = composer.ShowText(
                  "RadioButton:",
                  new PointF(140, 168),
                  XAlignmentEnum.Right,
                  YAlignmentEnum.Middle,
                  0
                  );
                var field = new RadioButton(
                  "myRadio",
                  /*
                    NOTE: A radio button field typically combines multiple alternative widgets.
                  */
                  new Widget[]
                  {
            new Widget(
              page,
              new RectangleF(150, 150, 36, 36),
              "first"
              ),
            new Widget(
              page,
              new RectangleF(200, 150, 36, 36),
              "second"
              ),
            new Widget(
              page,
              new RectangleF(250, 150, 36, 36),
              "third"
              )
                  },
                  "second" // Selected item (it MUST correspond to one of the available widgets' names).
                  ); // 4.1. Field instantiation.
                fields.Add(field); // 4.2. Field insertion into the fields collection.
                fieldStyle.Apply(field); // 4.3. Appearance style applied.
            }

            // 4.d. Text field.
            {
                _ = composer.ShowText(
                  "TextField:",
                  new PointF(140, 218),
                  XAlignmentEnum.Right,
                  YAlignmentEnum.Middle,
                  0
                  );
                var field = new TextField(
                  "myText",
                  new Widget(
                    page,
                    new RectangleF(150, 200, 200, 36)
                    ),
                  "Carmen Consoli" // Current value.
                  ); // 4.1. Field instantiation.
                field.SpellChecked = false; // Avoids text spell check.
                var fieldActions = new FieldActions(document);
                field.Actions = fieldActions;
                fieldActions.OnValidate = new JavaScript(
                  document,
                  "app.alert(\"Text '\" + this.getField(\"myText\").value + \"' has changed!\",3,0,\"Validation event\");"
                  );
                fields.Add(field); // 4.2. Field insertion into the fields collection.
                fieldStyle.Apply(field); // 4.3. Appearance style applied.
                var blockComposer = new BlockComposer(composer);
                blockComposer.Begin(new RectangleF(360, 200, page.Size.Width - 400, 36), XAlignmentEnum.Left, YAlignmentEnum.Middle);
                composer.SetFont(composer.State.Font, 7);
                _ = blockComposer.ShowText("If you leave this text field after changing its content, a javascript action should prompt you an alert box responding to the validation event triggered by your PDF viewer.");
                blockComposer.End();
            }

            // 4.e. Choice fields.

            // Preparing the item list that we'll use for choice fields (a list box and a combo box (see below))...
            var items = new ChoiceItems(document);
            _ = items.Add("Tori Amos");
            _ = items.Add("Anouk");
            _ = items.Add("Joan Baez");
            _ = items.Add("Rachele Bastreghi");
            _ = items.Add("Anna Calvi");
            _ = items.Add("Tracy Chapman");
            _ = items.Add("Carmen Consoli");
            _ = items.Add("Ani DiFranco");
            _ = items.Add("Cristina Dona'");
            _ = items.Add("Nathalie Giannitrapani");
            _ = items.Add("PJ Harvey");
            _ = items.Add("Billie Holiday");
            _ = items.Add("Joan As Police Woman");
            _ = items.Add("Joan Jett");
            _ = items.Add("Janis Joplin");
            _ = items.Add("Angelique Kidjo");
            _ = items.Add("Patrizia Laquidara");
            _ = items.Add("Annie Lennox");
            _ = items.Add("Loreena McKennitt");
            _ = items.Add("Joni Mitchell");
            _ = items.Add("Alanis Morissette");
            _ = items.Add("Yael Naim");
            _ = items.Add("Noa");
            _ = items.Add("Sinead O'Connor");
            _ = items.Add("Dolores O'Riordan");
            _ = items.Add("Nina Persson");
            _ = items.Add("Brisa Roche'");
            _ = items.Add("Roberta Sammarelli");
            _ = items.Add("Cristina Scabbia");
            _ = items.Add("Nina Simone");
            _ = items.Add("Skin");
            _ = items.Add("Patti Smith");
            _ = items.Add("Fatima Spar");
            _ = items.Add("Thony (F.V.Caiozzo)");
            _ = items.Add("Paola Turci");
            _ = items.Add("Sarah Vaughan");
            _ = items.Add("Nina Zilli");

            // 4.e1. List box.
            {
                _ = composer.ShowText(
                  "ListBox:",
                  new PointF(140, 268),
                  XAlignmentEnum.Right,
                  YAlignmentEnum.Middle,
                  0
                  );
                var field = new ListBox(
                  "myList",
                  new Widget(
                    page,
                    new RectangleF(150, 250, 200, 70)
                    )
                  ); // 4.1. Field instantiation.
                field.Items = items; // List items assignment.
                field.MultiSelect = false; // Multiple items may not be selected simultaneously.
                field.Value = "Carmen Consoli"; // Selected item.
                fields.Add(field); // 4.2. Field insertion into the fields collection.
                fieldStyle.Apply(field); // 4.3. Appearance style applied.
            }

            // 4.e2. Combo box.
            {
                _ = composer.ShowText(
                  "ComboBox:",
                  new PointF(140, 350),
                  XAlignmentEnum.Right,
                  YAlignmentEnum.Middle,
                  0
                  );
                var field = new ComboBox(
                  "myCombo",
                  new Widget(
                    page,
                    new RectangleF(150, 334, 200, 36)
                    )
                  ); // 4.1. Field instantiation.
                field.Items = items; // Combo items assignment.
                field.Editable = true; // Text may be edited.
                field.SpellChecked = false; // Avoids text spell check.
                field.Value = "Carmen Consoli"; // Selected item.
                fields.Add(field); // 4.2. Field insertion into the fields collection.
                fieldStyle.Apply(field); // 4.3. Appearance style applied.
            }

            composer.Flush();
        }

        public override void Run(
          )
        {
            // 1. PDF file instantiation.
            var file = new File();
            var document = file.Document;

            // 2. Content creation.
            this.Populate(document);

            // 3. Serialize the PDF file!
            _ = this.Serialize(file, "AcroForm", "inserting AcroForm fields", "Acroform, creation, annotations, actions, javascript, button, combo, textbox, radio button");
        }
    }
}