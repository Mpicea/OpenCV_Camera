# OpenCV_Camera

#### C#에 OpenCV를 활용하여 영상촬영에 다양한 효과를 제작한 프로젝트입니다.
|프로젝트|설명|작성한 분|
|:---:|:---:|:---:|
|Hyowon|안면인식과 눈을 인식하고 새 이미지를 띄웁니다.|장효원님|
|Bolrok_Ohmok.cs|볼록&오목렌즈 효과와 캐니에지를 적용하였습니다.|황형선님|
|CameraAppProject1.cs|통합 - 안면인식, 눈 인식, 볼록&오목렌즈, 블러, 샤프닝|문도원님|
|CameraSharpenBlur.cs|블러 효과와 샤프닝 효과를 적용하였습니다.|문도원님|
  
#### 각 프로젝트 생성 방법
1. Visual Studio 2022에서 Windows Forms 앱(.Net Framework)를 통해 새 프로젝트 생성
2. NuGet 패키지에서 OpenCvSharp4.Windows를 4.8.0 버전으로 설치
3. NuGet 패키지에서 OpenCvSharp4.Extensions를 4.8.0 버전으로 설치  
  
#### 특정 프로젝트 추가 설정 방법 (Hyowon / CameraAppProject1.cs)
1. 생성된 프로젝트 명으로 만들어진 폴더를 열기
2. ~/프로젝트 명/프로젝트 명/bin/Debug 폴더 경로로 이동
3. 깃허브의 Hyowon폴더에 있는 haarcascade_frontalface_default.xml 파일 다운 및 2번 경로에 저장
4. 깃허브의 Hyowon폴더에 있는 haarcascade_eye.xml 파일 다운 및 2번 경로에 저장
5. 깃허브의 Hyowon폴더에 있는 moon.jpg, fire.jpg 다운 및 2번 경로에 저장
