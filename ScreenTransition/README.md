# 화면 전환 시스템

씬 로드 · 테마 교체처럼 "화면이 바뀌는 순간"을 가리는 연출 시스템입니다. 호출부는 키(`string`) 하나만 넘기고, 어떤 연출을 어떤 파라미터로 재생할지는 전부 에셋(프로필)이 결정합니다. 새 연출을 추가해도 호출부 코드는 바뀌지 않습니다.

> 본 폴더는 실제 프로젝트에서 발췌한 소스로, 단독 컴파일 대상이 아닙니다.
> 비동기는 UniTask, 인스펙터는 Odin Inspector에 의존하며 `SingletonBehaviourBase<T>`는 프로젝트 공용 클래스라 포함하지 않았습니다.

## 코드 순서

| 순서 | 파일 | 내용 |
|---|---|---|
| 1 | `ScreenTransitioner.cs` | **진입점** — 키로 프로필을 찾아 효과를 생성 · 재생 · 정리 |
| 2 | `ScreenTransitionEffectBase.cs` | 효과 계약 — In/Out 두 개의 비동기 메서드 + 제네릭 설정 바인딩 |
| 3 | `ScreenTransitionSettings.cs` | 키 → 프로필 맵 (전환기가 참조하는 단일 에셋) |
| 4 | `ScreenTransitionProfile.cs` | 프리팹 + 그 연출 전용 설정 한 쌍 |
| 5 | `Effects/SimpleFadeEffect.cs` | 최소 구현 예시 — 계약을 채우는 데 필요한 코드의 하한선 |
| 6 | `Effects/PixelDissolveEffect.cs` · `Effects/PixelDissolve.shader` | 셰이더 기반 구현 예시 |

## 사용 흐름

Out과 In이 나뉘어 있어, 그 사이에 무거운 작업을 끼워 넣으면 그대로 가려집니다.

```csharp
string transitionKey = "TitleToStage";
await ScreenTransitioner.Instance.StartTransitionOutAsync(transitionKey); // 화면을 덮고 그대로 유지
await LoadSceneAsync(_stageScene);                                        // 로드는 가림막 뒤에서
await ScreenTransitioner.Instance.StartTransitionInAsync(transitionKey);  // 걷어내고 오브젝트까지 정리
```

## 핵심 포인트 위치

- **Out은 남기고 In은 파괴** — `ScreenTransitioner.StartTransitionAsync()`
  Out은 연출 오브젝트를 살려 둬 화면을 덮은 상태를 유지하고, In은 재생이 끝나면 파괴합니다.
  다음 전환 시작 시 `_lastTransition`을 파괴하는 것도 같은 맥락 — 덮여 있던 가림막을 새 전환이 이어받습니다.
- **연출마다 다른 설정을, 프로필 하나로** — `ScreenTransitionProfile.OnEffectPrefabChanged()`
  페이드는 `Duration` 하나면 되지만, 픽셀 디졸브는 블록 크기 · 방향 · 색까지 필요합니다. 연출마다 설정 항목이 다릅니다.
  그래서 프로필에 프리팹을 꽂는 순간 그 연출이 쓰는 설정 클래스를 찾아 자동으로 만들어 줍니다.
  기획자는 프리팹만 고르면 그에 맞는 설정 항목이 인스펙터에 그려지고, 설정 타입을 직접 고를 일이 없습니다.
- **캐스팅은 베이스 클래스 한 줄에서만** — `ScreenTransitionEffectBase<T>.SetSettings()`
  프로필은 어떤 연출의 설정이든 담아야 하므로 설정을 `object`로 보관합니다. 그래서 효과에 넘길 때 한 번은 캐스팅이 필요합니다.
  그 캐스팅은 베이스 클래스 `ScreenTransitionEffectBase<T>`가 도맡아, `object`를 `T`로 한 번 바꿔 `Settings`에 담아 둡니다.
  각 연출 구현체는 캐스팅도 타입 검사도 없이, 자기 설정 타입 그대로인 `Settings`만 씁니다.
- **씬 로드 직후 델타 스파이크 방어** — `ScreenTransitionEffectBase.ClampedDeltaTime`
  전환은 `Time.timeScale`에 흔들리면 안 되므로 `unscaledDeltaTime`으로 진행하는데,
  씬 로드 직후 첫 프레임의 델타가 크게 튀어 연출이 한 프레임에 끝나버립니다. 1/30초로 클램프해 **모든 효과가 공유**하도록 베이스에서 처리합니다.
