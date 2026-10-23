# Direktori Laporan Kesiapan Pull Request Backend (Pre-Deploy Reports)

Direktori ini menyimpan seluruh laporan hasil audit kesiapan pra-Pull Request dan pra-deployment (*pre-deployment check*) untuk backend Quilvian (`NewQuilvianSystemBackend`).

Laporan dihasilkan secara otomatis setiap kali Skill **`pre-deploy-check-backend`** dijalankan oleh Google Antigravity / AI Agent sebelum pengembang melakukan `git push` dan mengajukan Pull Request ke branch integrasi tim **`QuilvianIntegrationBackEnd`**.

---

## Standar Penamaan Berkas Laporan

Karena pengerjaan backend dilakukan secara kolaboratif bersama tim di masing-masing branch kerja, format penamaan berkas laporan wajib menyertakan **nama branch pengembang**:
```text
pre-deploy-report-<nama-branch>-YYYYMMDD-HHmmss.md
```
Contoh:
- `pre-deploy-report-MHamzah-20261023-101500.md`
- `pre-deploy-report-AndryZain-20261024-093000.md`

---

## 4 Gerbang Kesiapan yang Diuji

Setiap berkas laporan mendokumentasikan hasil pengujian dari 4 gerbang berikut:

1. **Gerbang 1: Sanitasi Git, Deteksi Branch, & Marker Konflik**: Mendeteksi branch pengembang dan memastikan working tree bersih serta bebas dari sisa konflik git (`<<<<<<<`, `=======`, `>>>>>>>`).
2. **Gerbang 2: Kompilasi .NET**: Menjalankan `dotnet build` untuk memastikan kompilasi 100% bebas error (*0 Error*) secara penuh dan utuh.
3. **Gerbang 3: Migrasi Skema Database EF Core**: Menjalankan `dotnet ef database update --no-build` untuk memverifikasi kesiapan skema database tanpa melakukan kompilasi ganda.
4. **Gerbang 4: Kepatuhan Standar Arsitektur QBE**: Menjalankan `.\tooling\qbe\Invoke-QbeConformanceCheck.ps1 -Mode Strict` untuk memverifikasi kepatuhan terhadap kontrak arsitektur Quilvian Backend.

---

## Status Kelulusan

Jika seluruh 4 gerbang lulus (termasuk tindakan auto-repair jika sempat ada error), laporan akan bertanda:
```text
✅ READY TO PULL REQUEST with QuilvianIntegrationBackEnd
```
Pengembang dapat langsung melakukan push branch dan membuka PR ke `QuilvianIntegrationBackEnd`.
