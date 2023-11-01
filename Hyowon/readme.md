### 실행 전에 해야할 작업  
1. Visual Studio 2022 에서 Windows Forms 앱 (.NET Framework)를 통해 새 프로젝트 생성
2. 생성된 프로젝트 명으로 폴더 경로 가기
3. ~/프로젝트 명/프로젝트 명/bin/Debug 폴더 경로로 이동
4. haarcascade_frontalface_default.xml 파일 생성 및 해당 경로에 저장
5. haarcascade_eye.xml 파일 생성 및 해당 경로에 저장
6. moon.jpg, fire.jpg 다운받고 해당 경로에 저장  

### 각 프로젝트  
* Facedetect.cs : 안면인식, 눈인식 - 인식한 곳에 이미지 띄우기
* HistogramEqualization.cs : 히스토그램 평활화
