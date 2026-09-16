# Roadmap Delivery Frontend — Modul Platform / Alokasi Nomor Bisnis

## Metadata

```yaml
module_id: platform
module_name: NumberSeriesManagement
blueprint_id: PLT-BP-001
blueprint_shape: SINGLE
blueprint_root: docs/module-blueprints/platform/
roadmap_revision: 1
status: DRAFT
approval_gate: BLUEPRINT_APPROVED
contract_version: v1 (approved)
frontend_source_sha: 101ec5d3a560bd6e54d4665ae53d425f255c609f
frontend_branch: sukmagpV2
backend_source_sha: f0d6855
backend_branch: sukmagp
decision_revision: 4
owners:
  - "UI/UX: belum ada brief navigasi yang disetujui"
approved_by:
  - "Sukma Giri Pratama (sukmagp) — blueprint PLT-SLICE-01 & set kontrak v1, 2026-09-09"
approved_at: "2026-09-09"
```

---

## 0. Peringatan yang tidak boleh dilewati

**Modul ini nyaris tidak punya frontend, dan itu benar.** Kemampuan intinya — mengalokasikan nomor
— **tidak punya layar sama sekali**. Alokasi terjadi di dalam pekerjaan modul lain; petugas tidak
pernah membukanya, menekannya, atau memintanya.

Yang ada hanya **satu layar pemantauan baca-saja**, dan itu pun untuk administrator sistem yang
sedang menelusuri keluhan, bukan untuk petugas harian.

**Gerbang `G4` Bank Darah tertutup tanpa satu layar pun dari roadmap ini.**

**Rupa layar `DEV_DISCRETION`.** Roadmap ini mengunci sumber data, hak akses, dan keadaan layar
yang wajib ada — bukan warna, tata letak, atau pilihan komponen.

---

## 1. Ringkasan status

| Penanda | Jumlah | Task |
| --- | ---: | --- |
| ⛔ BLOCKED | 1 | `PLT-FE-001` |
| **Total** | **1** | |

Satu task, dan ia **`SHOULD HAVE`** — bukan `MUST HAVE`.

---

## 2. Urutan dependency

```text
✅ PLT-BE-003 (alokator) ──> 🟡 PLT-BE-005 (endpoint baca) ──> ⛔ PLT-FE-001 (layar pemantauan)
                                                                    gelombang MVP-2
```

---

## 3. Task

### ⛔ `PLT-FE-001` — Administrator membaca keadaan deret nomor

| Field | Isi |
| --- | --- |
| **Status** | ⛔ **BLOCKED** — pasangan backend `PLT-BE-005` belum ada. Gelombang `MVP-2`, **`SHOULD HAVE`** |
| **Yang memblokir** | `PLT-BE-005` saja. `PLT-BE-003` ✅ selesai dan gerbang `P1` ✅ tertutup, sehingga `PLT-BE-005` kini 🟡 siap dikerjakan |
| **Pekerjaan yang tetap aman** | Nol. Layar tanpa endpoint tidak dapat dibangun, dan kontrak melarang mendahului backend |
| **Outcome** | Administrator dapat melihat deret nomor beserta nilai pencacah dan waktu alokasi terakhir saat menelusuri keluhan |
| **Trace** | `FR-PLT-012`, `FR-PLT-013` |
| **Layar** | `PLT-FE-01` |
| **Kontrak** | api-contract `v1` — empat endpoint, **seluruhnya baca** |
| **Dependency** | 🟡 `PLT-BE-005` |
| **Acceptance** | Kolom nilai berlabel jelas sebagai **nomor terakhir yang sudah terbit**, bukan nomor berikutnya; keadaan kosong berbunyi "Belum ada deret nomor yang pernah dipakai" dan diperlakukan sebagai keadaan **wajar**, bukan galat |
| **Verification** | Butir menu tidak muncul bagi pengguna tanpa `NumberSeries : Read`; route langsung ditolak backend |
| **Risk/owner** | Rendah / administrator sistem |
| **DoD** | **Nol tombol aksi** — tidak ada Tambah, Ubah, Hapus, maupun Setel Ulang. Bukan karena belum dibuat, melainkan karena backend memang tidak menyediakannya; menyetel ulang pencacah melanggar `INV-PLT-001` |
| **`DEV_DISCRETION`** | Warna, jarak, ikon, dan component library. **Bukan** `DEV_DISCRETION`: sumber data, butir hak akses, dan bunyi keadaan kosong |

---

## 4. Wewenang UI yang belum diturunkan

| Hal | Keadaan |
| --- | --- |
| Penempatan butir menu | **Usulan** `/pengaturan/deret-nomor` di bawah rumpun pengaturan sistem. Belum ada brief navigasi yang disetujui |
| Perlu-tidaknya layar ini sama sekali | **Terbuka.** Slice ini berguna penuh tanpa layar; bila pemilik memutuskan layar tidak perlu, `PLT-FE-001` dan `PLT-BE-005` dicabut bersama |
| Peringatan deret hampir habis | **Tidak dirancang** — `OQ-PLT-005` terbuka. Layar menampilkan nilai apa adanya tanpa ambang |
