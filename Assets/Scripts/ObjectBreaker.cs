using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBreaker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    // ObjectBreaker スクリプトをアタッチしたオブジェクトが別のオブジェクトと接触した時に呼ばれる
    void OnCollisionEnter(Collision collision)
    {
        BreakThisGameObject();
    }

    private void BreakThisGameObject()
    {
        // 破片のオブジェクトを新規に作る
        createPieces();

        // 自身のオブジェクトは消す
        Destroy(this.gameObject);
    }

    private void createPieces()
    {
        Mesh mesh = this.gameObject.GetComponent<MeshFilter>().mesh;
        int[] triangles = mesh.triangles;
        for(int i = 0; i < triangles.Length; i += 3)
        {
            int pieceIndex = i / 3;
            int[] triangle = { triangles[i], triangles[i+1] , triangles[i+2] };

            //１つの三角形ポリゴンを元に、１つの破片オブジェクトを作る
            createOnePieceFromTriangle(pieceIndex, triangle);
        }
    }

    private void createOnePieceFromTriangle(int pieceIndex, int[] triangle)
    {
        GameObject piece = new GameObject("piece_" + pieceIndex);

        piece.transform.position = this.gameObject.transform.position;
        piece.transform.rotation = this.gameObject.transform.rotation;
        piece.transform.localScale = this.gameObject.transform.localScale;

        //破片オブジェクトとして、テトラ要素を作る
        Mesh pieceMesh = piece.AddComponent<MeshFilter>().mesh;
        setTetraMeshFromTriangle(ref pieceMesh, ref triangle);

        Material pieceMaterial = piece.AddComponent<MeshRenderer>().material;
        pieceMaterial = this.gameObject.GetComponent<MeshRenderer>().material;

        piece.AddComponent<Rigidbody>();

        MeshCollider pieceMeshCollider = piece.AddComponent<MeshCollider>();
        pieceMeshCollider.sharedMesh = pieceMesh;
        pieceMeshCollider.convex = true;

        piece.AddComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>();
    }

    private void setTetraMeshFromTriangle(ref Mesh tetraMesh, ref int[] triangle) {
        Vector3[] tetraMeshVertices = calculateTetraMeshVertices(ref triangle);
        int[] tetraMeshTriangles = {0,  1,  2,
                                    5,  4,  3,
                                    6,  7,  8,
                                    11, 10, 9};
        Vector2[] tetraMeshUV = calculateTetraMeshUV(ref triangle);

        tetraMesh.vertices = tetraMeshVertices;
        tetraMesh.triangles = tetraMeshTriangles;
        tetraMesh.uv = tetraMeshUV;
        tetraMesh.RecalculateBounds();
        tetraMesh.RecalculateNormals();
    }

    private Vector3[] calculateTetraMeshVertices(ref int[] triangle) {
        Vector3[] tetraVertices = new Vector3[4];
        Vector3[] thisGameObjectVertices = this.gameObject.GetComponent<MeshFilter>().mesh.vertices;
        tetraVertices[0] = thisGameObjectVertices[triangle[0]];
        tetraVertices[1] = thisGameObjectVertices[triangle[1]];
        tetraVertices[2] = thisGameObjectVertices[triangle[2]];
        Vector3[] thisGameObjectNormals = this.gameObject.GetComponent<MeshFilter>().mesh.normals;
        Vector3 triangleNormal = thisGameObjectNormals[triangle[0]] +
                                 thisGameObjectNormals[triangle[1]] +
                                 thisGameObjectNormals[triangle[2]];
        triangleNormal.Normalize();

        Vector3 thisGameObjectBounds = this.gameObject.GetComponent<MeshFilter>().mesh.bounds.size;
        float shiftValue = (thisGameObjectBounds.x + thisGameObjectBounds.y + thisGameObjectBounds.z) / 3.0f * 0.3f;

        // 三角形ポリゴンの幾何中心を法線方向にずらした位置をテトラ要素の 4 点目にする
        Vector3 triangleCentroid = (tetraVertices[0] + tetraVertices[1] + tetraVertices[2]) / 3.0f;
        tetraVertices[3] = triangleCentroid - triangleNormal * shiftValue;

        Vector3[] ret = { tetraVertices[0], tetraVertices[1], tetraVertices[2],
                          tetraVertices[0], tetraVertices[1], tetraVertices[3],
                          tetraVertices[0], tetraVertices[2], tetraVertices[3],
                          tetraVertices[1], tetraVertices[2], tetraVertices[3]};
        return ret;
    }

    private Vector2[] calculateTetraMeshUV(ref int[] triangle)
    {
        Vector3[] tetraUV = new Vector3[4];
        Vector2[] thisGameObjectUV = this.gameObject.GetComponent<MeshFilter>().mesh.uv;
        tetraUV[0] = thisGameObjectUV[triangle[0]];
        tetraUV[1] = thisGameObjectUV[triangle[1]];
        tetraUV[2] = thisGameObjectUV[triangle[2]];
        tetraUV[3] = (tetraUV[0] + tetraUV[1] + tetraUV[2]) / 3.0f;
        Vector2[] ret = { tetraUV[0], tetraUV[1], tetraUV[2],
                          tetraUV[0], tetraUV[1], tetraUV[3],
                          tetraUV[0], tetraUV[2], tetraUV[3],
                          tetraUV[1], tetraUV[2], tetraUV[3]};
        return ret;
    }
}
