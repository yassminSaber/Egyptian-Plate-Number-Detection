using Compunet.YoloV8;
using System.Drawing;
using System.Runtime.Versioning;

namespace InferenceLocal
{
    class InferenceLocal{
        [SupportedOSPlatform("windows")]
        static Bitmap CropImage(System.Drawing.Image originalImage, SixLabors.ImageSharp.Rectangle rect){
            // Create a new bitmap with the specified crop dimensions
            Bitmap croppedImage = new Bitmap(rect.Width, rect.Height);
            // Create a graphics object from the cropped image
            using (Graphics g = Graphics.FromImage(croppedImage)){
                // Draw the portion of the original image onto the cropped image
                g.DrawImage(originalImage,  new System.Drawing.Rectangle(0,0,rect.Width,rect.Height),rect.Left,rect.Top,rect.Width,rect.Height,GraphicsUnit.Pixel);
            }

            return croppedImage;
        }
    
            [SupportedOSPlatform("windows")]
        static void  Main(string[] args){
                // Model 1
            string imagePath = @"C:\Users\yasme\OneDrive\Desktop\plateNumber\img.jpg";
            using var predictor = new YoloV8(@"C:\Users\yasme\OneDrive\Desktop\plateNumber\Model1.onnx");  // load the first model 
            var result = predictor.Detect(imagePath); 
            
            System.Drawing.Image image2=System.Drawing.Image.FromFile(imagePath);
            using var ploted =CropImage(image2,result.Boxes[0].Bounds);  // crop the plate number 
            ploted.Save(@"C:\Users\yasme\OneDrive\Desktop\plateNumber\plate_cropped.jpg"); // save the plate number image 

                // Model 2 
            using var model2 = new YoloV8(@"C:\Users\yasme\OneDrive\Desktop\plateNumber\Model2.onnx");  // load the second model
            var result2 = model2.Detect(@"C:\Users\yasme\OneDrive\Desktop\plateNumber\plate_cropped.jpg");
            
            Dictionary<int,string> resultDict  =new Dictionary<int,string>(); // contains the class name and the left coordinate

            foreach(var b in result2.Boxes){
                string className = b.Class.Name; 
                int leftPos = b.Bounds.Left;
                resultDict.Add(leftPos,className);
            }
            Dictionary<int,string> sorted_resultDict = resultDict.OrderBy(kvp=>kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value); // sort the dict based on the left coordinate
    
            List<int> classNameList = Enumerable.Range(0, 27).ToList(); // a list with classes Name
            classNameList.Remove(17);
            
            List<string> mappingNames = new List<string>();
            mappingNames.AddRange(new[] {"ا","ب","ت","د","ر","س","ص","ط","ع","ف","ق",
                                        "ل","م","ن","هـ","و","ى","١","٢","٣","٤","٥","٦","٧","٨","٩"}); // list with the actual names for the classes
            
            Dictionary<int,string> mapClassName = new Dictionary<int, string>(); // combine the class name and the actual name for mapping
            int n = classNameList.Count;
            for(int i = 0; i <n ;i++){
                int name = classNameList[i];
                string mappedName = mappingNames[i];
                mapClassName.Add(name,mappedName);
            }
           // mapping process
            List<string> finalResult = new List<string>();
            string txt="";
            foreach(var kvp in sorted_resultDict){
                string value = kvp.Value;
                int intValue = int.Parse(value);
                string stringName= mapClassName[intValue];
                finalResult.Add(stringName);
            }
            List<string> reversedList = finalResult.AsEnumerable().Reverse().ToList();
            foreach(string i in reversedList){
                 txt+=i+" ";
            }
            Console.WriteLine(txt);
            }               
        }
    }

