using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using DG.Tweening;
using Unity.VisualScripting;

public class BrushTrailTracker : MonoBehaviour
{
    [Header("TrailPrefab")]
    [SerializeField] private TrailRenderer _trailPrefab;
    [FormerlySerializedAs("mainCamera")]
    [Header("メインカメラ")]
    [SerializeField] private Camera _mainCamera;
    [Header("始まりの太さ")]
    [SerializeField] private float _startWidth = 0.3f;
    [Header("終わりの太さ")]
    [SerializeField] private float _endWidth = 0.5f;
    [Header("消えているときのCutOff")]
    [SerializeField] private float _disappearingCutOff = 10;
    [Header("現れるときのCutOff")]
    [SerializeField] private float _appearingCutOff;
    [Header("アニメーションのインターバル")]
    [SerializeField] private float _intervalTime = 0.5f;
    
    private TrailRenderer _currentTrail; // 現在のトレイル
    private Vector3 _mousePosition;
    private bool _isDrawing;
    
    private List<Material> _traiMaterialslList = new List<Material>();
    private bool _isDirection;
    private int _animationCount = 0;
    
    
    private void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f; // カメラからの距離を設定
        _mousePosition = _mainCamera.ScreenToWorldPoint(mousePos);
        this.transform.position = _mousePosition;

        //演出中には入力を受付けないようにする
        if (_isDirection)
            return;
        
        // 左クリックしている間にトレイルを描く
        if (Input.GetMouseButton(0)) // 左クリック
        {
            DrawingBrush();
        }
        else if (Input.GetMouseButtonUp(0)) // クリックを離したらトレイルを終了
        {
            EndCurrentTrail();
        }
        else if (Input.GetMouseButton(1))
        {
            AppearanceText();
        }
    }

    private void DrawingBrush()
    {
        if (!_isDrawing)
        {
            StartNewTrail(_mousePosition); // マウス位置で新しいトレイルを開始
        }
        else
        {
            _currentTrail.transform.position = _mousePosition;
        }
        
    }

    // 新しいトレイルを開始
    private void StartNewTrail(Vector3 startPosition)
    {
        _isDrawing = true;
        
        // 新しいトレイルオブジェクトを生成
        _currentTrail = Instantiate(_trailPrefab, startPosition, Quaternion.identity);

        // トレイルの設定を変更
        _currentTrail.startWidth = _startWidth;
        _currentTrail.endWidth = _endWidth;
        _currentTrail.time = Mathf.Infinity; // トレイルが消えないように無限に設定

        // トレイルのマテリアルを取得
        Material newMaterial = new Material(_currentTrail.material);
        _currentTrail.material = newMaterial;
        newMaterial.SetFloat("_Cutoff", _disappearingCutOff);
        _traiMaterialslList.Add(newMaterial);
    }
    
    // トレイルを終了して新しいトレイルを書く準備をする
    private void EndCurrentTrail()
    {
        if (_currentTrail != null)
        {
            _isDrawing = false;
            _currentTrail = null; // 現在のトレイルをリセット
        }
    }

    //文字を表示させる
    private async UniTask AppearanceText()
    {
        _isDirection = true;
        // 各マテリアルに同時にアニメーションを適用
        List<Tweener> tweens = new List<Tweener>();

        int count = 0;
        foreach (var material in _traiMaterialslList)
        {
            DOVirtual.Float(_disappearingCutOff, _appearingCutOff, _intervalTime,
                value => material.SetFloat("_Cutoff", value));

            await UniTask.Delay(TimeSpan.FromSeconds(_intervalTime - 0.2f));
        }
        
        await UniTask.Delay(TimeSpan.FromSeconds(3));
        
        foreach (var material in _traiMaterialslList)
        {
            DOVirtual.Float(_appearingCutOff, _disappearingCutOff, _intervalTime,
                value => material.SetFloat("_Cutoff", value));

            await UniTask.Delay(TimeSpan.FromSeconds(_intervalTime - 0.2f));
        }
        
        await UniTask.Delay(TimeSpan.FromSeconds(1));
        ResetTrail();
        _isDirection = false;
    }

    private void ResetTrail()
    {
        // すべてのトレイルオブジェクトを削除
        foreach (var trail in FindObjectsOfType<TrailRenderer>())
        {
            Destroy(trail.gameObject); // トレイルオブジェクト自体を削除
        }

        // リスト内のすべてのマテリアルを削除
        foreach (var material in _traiMaterialslList)
        {
            Destroy(material);
        }

        // マテリアルリストをクリア
        _traiMaterialslList.Clear();
    }
}
