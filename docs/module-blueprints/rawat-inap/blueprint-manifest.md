# Rawat Inap — Blueprint Manifest

| Field | Value |
|---|---|
| `blueprint_id` | `RWI-BP-001` |
| `revision` | `1` |
| `status` | `draft` — **belum disetujui manusia** |
| `module` | `rawat-inap` / `InPatientManagement`, prefix entity `Inp` |
| `registry_lifecycle` | `PLANNED` — hanya memberi hak penamaan, belum memberi izin implementasi |
| `design_snapshot_at` | `2026-08-21` |
| `backend_commit_sha` | `5afb54bd75281648010e50ef14f43ca1f80d8efd` (branch `MHamzah`) |
| `frontend_commit_sha` | `dec4fdeff07c3c96ad9f07f41f184c54cf771371` (branch `HamzahV2`) |
| `owners` | Product/Domain: pemilik suite skill sebagai **pemegang sementara** sesuai `RWI-DEC-006`, nama belum diisi. Clinical governance: `OPEN`. Security/Privacy: `OPEN`. API dan Frontend authority: sesuai decision log |
| `approved_by` | **Belum ada.** Approval adalah tindakan manusia |
| `approved_at` | — |
| `requirement_readiness` | `PARTIALLY_READY` |
| `domain_architecture_revision` | `0.1` |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_PARTIAL` |
| `scope` | Sembilan slice yang dinyatakan siap dan berdiri sendiri. Delapan slice lain sengaja tidak dirancang |
| `compatibility_impact` | Dua belas tabel baru, **nol** perubahan kolom pada tabel modul lain. Satu perubahan **perilaku** pada `PATCH /beds/{id}/availability`. Satu perbaikan pemanggilan di frontend |

---

## 0. Status kesegaran — **STALE**

> **Blueprint ini tertinggal dari masukannya.** Pada 2026-08-21, setelah blueprint revision `1`
> selesai, Amendment Pass `/grill-me` menaikkan `00-interview-decisions.md` ke revision `3` dan
> menghasilkan empat keputusan yang **mengubah desain**:
>
> | Keputusan | Yang berubah |
> |---|---|
> | `RWI-DEC-054` | Invariant baru `INV-INP-10` satu pasien satu episode aktif, beserta aturan validasi dan unique index parsial |
> | `RWI-DEC-055` | Kepergian fisik pasien melepas tempat tidur lebih awal. Melonggarkan `INV-INP-01`, menambah kolom, perintah, nilai enum, dan endpoint |
> | `RWI-DEC-056` | Kolom opsional rujukan episode ibu pada episode bayi |
> | `RWI-DEC-057` | Tabel salinan versi resume pulang |
>
> Keempatnya **belum** masuk ke `02-backend-architecture.md`, `erd/`, `contracts/`, `testing/`,
> maupun `04-prd-to-mvp.md`. Blueprint wajib naik ke revision `2` lewat `/qv-design` sebelum
> dipakai sebagai dasar perencanaan atau implementasi.
>
> `RWI-DEC-053` sengaja dipilih supaya tidak mengubah apa pun: riwayat lokasi tetap milik Rawat
> Inap, sehingga catatan penempatan dan seluruh kontrak yang sudah disusun tetap berlaku.

---

## 1. Peringatan sebelum membaca

Seluruh dokumen pada folder ini berstatus `draft`. Tidak satu pun boleh dipakai sebagai izin
menulis source code.

Empat gerbang implementasi masih terbuka, dan modul `InPatientManagement` masih berstatus `PLANNED`
pada registry.

---

## 2. Daftar artefak dan hash

| Artefak | Revision | Status | SHA-256 |
|---|---|---|---|
| [`00-interview-decisions.md`](./00-interview-decisions.md) | `3` | `draft` | `bbbd30c38bd14a3b8672db13a8abd1a7fe9a79c207f2e87a06a3c5b2cf9ef765` |
| [`01-existing-capability-map.md`](./01-existing-capability-map.md) | `1.2` | `source-audited` | `567d7f7ea57537f419efca28d551e965524d27ea1889a00cc7707d17ec74c3b6` |
| [`evidence/02-requirement-completeness-gate.md`](./evidence/02-requirement-completeness-gate.md) | `1.0` | `CURRENT` | `cc32db172b2441b2967ce3507c89b81f12fc103bbd3b3a92bc7bc49d77005ffe` |
| [`evidence/03-hospital-domain-architecture.md`](./evidence/03-hospital-domain-architecture.md) | `0.1` | `draft` | `721268f11edd4aff047b6fcf03fce28e4f051cb4d1cf5134c32d11f0f52615d3` |
| [`02-backend-architecture.md`](./02-backend-architecture.md) | `0.1` | `draft` | `b2eb2f1d2432877c98fdebb4666cd29299deea12cf1a6813c5e8fc6d92aa315e` |
| [`03-frontend-architecture.md`](./03-frontend-architecture.md) | `0.1` | `draft` | `8db0abf8150d8a89b200bbf62fb3c00be4e017eab49ad6f0478f66b8ecfd44d8` |
| [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) | `0.1.0` | `draft` | `b5073571a095faa9d968f955527dd569d88be40d271c3dc0aaf7ae9200f94692` |
| [`erd/00-context-erd.md`](./erd/00-context-erd.md) | `0.1` | `draft` | `c3b55556ae1b1d3115628e091162f53aad2aa92bbeb699b41dd2ea1dde232cf3` |
| [`erd/01-inpatient-episode.md`](./erd/01-inpatient-episode.md) | `0.1` | `draft` | `3b955f06319745a828b47248275323f9f2db0c63a0fd8a744f4874b9d7d15624` |
| [`erd/02-inpatient-configuration.md`](./erd/02-inpatient-configuration.md) | `0.1` | `draft` | `3645ee9d1788270ee7cef88d2cc6b74beddddec0a1a5d2b538e45c25c66f2065` |
| [`erd/data-dictionary.md`](./erd/data-dictionary.md) | `0.1` | `draft` | `ec9997293198b3887ee1e0f155bc8ff5439f11c07826aa43ae4569f4ea744a95` |
| [`contracts/api-contract.md`](./contracts/api-contract.md) | `0.1.0` | `draft` | `bf6aa62c172dd3ec73f0ef9bd6ed6e3ac2f79b466ad254f9b2638b02f847da10` |
| [`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md) | `0.1.0` | `draft` | `e7ace1c43d7e21f7e81aea0fcc13e0249f574356870e38d4fe3b9e21dd9f55de` |
| [`contracts/validation-matrix.md`](./contracts/validation-matrix.md) | `0.1.0` | `draft` | `49703d568ee811b7c971cb0b3160ba9b3274759443979a9666f824f3df596950` |
| [`contracts/integration-contract.md`](./contracts/integration-contract.md) | `0.1.0` | `draft` | `bf347fafe8e6a7642e0c0e62ad9592321b995208b02068b2a1a780c94dd8109d` |
| [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md) | `0.1.0` | `draft` | `6294494eac52b2b197a537cca5e013d161375ce52ae35481dabedfa54a2b84e0` |
| [`testing/acceptance-test-matrix.md`](./testing/acceptance-test-matrix.md) | `0.1.0` | `draft` | `47d145de92271a83c0427b91d7d5adf16baf87cf8e60373a0d902a105c896051` |

Hash di atas dipakai mendeteksi perubahan yang tidak tercatat. Bila salah satu berubah tanpa
revision naik, blueprint dianggap tidak konsisten.

---

## 3. Contract version

| Kontrak | Version | Status |
|---|---|---|
| API | `0.1.0` | `draft` |
| State transition | `0.1.0` | `draft` |
| Validation | `0.1.0` | `draft` |
| Integration | `0.1.0` | `draft` |
| Permission dan audit | `0.1.0` | `draft` |
| Acceptance test | `0.1.0` | `draft` |
| PRD ke MVP | `0.1.0` | `draft` |

---

## 4. Rantai masukan

```text
00-interview-decisions.md  rev 3  (grill-me: Scope + Closure + Amendment Pass)
        |
01-existing-capability-map.md  (trace-existing-capabilities)
        |
evidence/02-requirement-completeness-gate.md  (PARTIALLY_READY)
        |
evidence/03-hospital-domain-architecture.md  (DOMAIN_ARCHITECTURE_PARTIAL)
        |
02-backend-architecture.md + 03-frontend-architecture.md + erd/ + contracts/ + testing/
        |
04-prd-to-mvp.md
```

Setiap tahap hanya meneruskan slice yang dinyatakan siap oleh tahap sebelumnya.

---

## 5. Scope yang dirancang

| Slice | Nama | Epic |
|---|---|---|
| `INP-S01` | Admisi dan pemesanan tempat tidur | `EPIC RI-21`, `EPIC RI-22` |
| `INP-S02` | Penempatan, census, lama dirawat | `EPIC RI-23`, `EPIC RI-24` |
| `INP-S03` | Perpindahan dan pindah kelas | `EPIC RI-26` |
| `INP-S04` | Penugasan perawat | `EPIC RI-25` |
| `INP-S07` sebagian | Keputusan pulang dan resume, tiga cara pulang | `EPIC RI-27` |
| `INP-S08` sebagian | Daftar periksa, kelayakan keuangan, penutupan | `EPIC RI-28` |
| `INP-S12` | Bayi baru lahir dan boks bayi | `EPIC RI-33` |
| `INP-S13` | Riwayat status, audit, dua daftar pantau | `EPIC RI-29`, `EPIC RI-30` |
| `INP-S14` | Pengaturan yang dapat diubah admin | `EPIC RI-31` |
| — | Perbaikan tempat tidur dan pembatasan wewenang status | `EPIC RI-32` |

## 6. Scope yang sengaja tidak dirancang

| Slice | Decision ID |
|---|---|
| `INP-S05` dokumentasi klinis dan visite | `DEC-INP-001` |
| `INP-S06` resep dan obat pulang | `DEC-INP-001` |
| `INP-S09` serah terima IGD | `DEC-INP-002` |
| `INP-S10` persetujuan umum | `DEC-INP-003` |
| `INP-S11` jenis kelamin dan isolasi | `DEC-INP-004` |
| `INP-S15` interoperabilitas SATUSEHAT | `DEC-INP-005` |
| Serah terima klinis antar shift | `DEC-INP-006` |
| Cara pulang meninggal dan kabur | `DEC-INP-007` |

---

## 7. Design gate

Blueprint ini adalah desain target, bukan spesifikasi implementasi yang disetujui.

### 7.1 Gerbang implementasi — desain boleh, source code belum

| Gate | Keterangan |
|---|---|
| Persetujuan pemilik modul tetangga | Empat modul `ACTIVE` akan disentuh. Lihat `RWI-OQ-032`, `RWI-OQ-033`, `RWI-OQ-034` |
| Kesiapan data master | Kamar dan tempat tidur harus terisi lewat layar aplikasi. Lihat `RWI-DEC-048`, `RWI-OQ-036` |
| Perbaikan tombol tempat tidur | Hari ini selalu gagal 404. Lihat `RWI-DEC-049` |
| Test regresi modul tetangga | Tidak ada satu pun test yang menjaga jalur poliklinik, IGD, dan farmasi. Lihat `RWI-DEC-051` |
| Registry lifecycle | Modul masih `PLANNED` |

### 7.2 Gerbang sebelum produksi — klinis dan privasi

| Gate | Keterangan |
|---|---|
| Clinical governance owner | Belum ditunjuk |
| Security/privacy owner | Belum ditunjuk |
| `RWI-RULE-012` isolasi dan jenis kelamin | Gerbang keras. `DEC-INP-004` |
| `RWI-RULE-021` batas waktu klinis | Gerbang keras |
| `RWI-RULE-025` persetujuan umum | Gerbang keras. `DEC-INP-003` |
| Masa simpan riwayat | `RWI-OQ-035`, keputusan hukum |

---

## 8. Yang tidak boleh diubah blueprint hilir

Diwariskan dari arsitektur domain bagian N.5. Perubahan pada butir berikut wajib kembali ke skill
hulu, bukan diselesaikan pada tahap perencanaan atau implementasi:

1. Kepemilikan data pada tabel kepemilikan `02-backend-architecture.md` bagian 2.
2. Kedudukan `MstBed.BedStatus` sebagai **salinan**, bukan sumber kebenaran.
3. Sepuluh invariant `INV-INP-01` sampai `INV-INP-09` beserta cara menjaganya.
4. Bentuk **berperiode** pada `InpDoctorAssignment`, `InpNurseAssignment`, dan `InpBedPlacement`.
   Menggantinya dengan satu kolom yang ditimpa akan menghapus riwayat yang dibutuhkan resume,
   billing, dan interoperabilitas.
5. Kedudukan `InpCorrectionSession` sebagai konsep tersendiri, bukan status episode keenam.

---

## 9. Pemicu impact scan

Blueprint ditandai stale dan wajib melewati impact scan bila salah satu berikut berubah:

| Yang berubah | Yang harus ditinjau ulang |
|---|---|
| `backend_commit_sha` atau `frontend_commit_sha` | Capability map lebih dulu, lalu seluruh kontrak |
| `Areas/HealthServices/MasterData/` tempat tidur, kamar, unit layanan, kelas | `erd/`, `contracts/api-contract.md`, `EPIC RI-22`, `RI-23`, `RI-32` |
| `Areas/HealthServices/RegistrationManagement/` | `INV-INP-04`, `EPIC RI-21` |
| `Areas/HealthServices/ClinicalManagement/` atau `PharmacyManagement/` | `DEC-INP-001`; slice yang dihentikan mungkin dapat dibuka |
| `Areas/HealthServices/BillingManagement/` | `RWI-RULE-028` aturan 7; sumber kelayakan keuangan mungkin berpindah |
| `Repositories/ApplicationDbContext.cs` | Rencana migration |
| `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Prefix dan lifecycle modul |
| Munculnya berkas berawalan `Inp` | Seluruh status `Baru` wajib dinilai ulang |

---

## 10. Riwayat revision

| Revision | Tanggal | Ringkasan |
|---|---|---|
| `1` | 2026-08-21 | Blueprint pertama. Dua bounded context, satu aggregate root, dua belas tabel baru, nol perubahan kolom pada tabel modul lain. Sembilan slice dirancang, delapan sengaja dihentikan. 13 epic, 47 functional requirement, 23 skenario UAT, 82 skenario acceptance test |

---

## 11. Langkah berikutnya

| Kondisi | Skill |
|---|---|
| Empat pertanyaan memblokir pada `04-prd-to-mvp.md` bagian 20.2 sudah terjawab dan owner menyetujui blueprint | `/qv-plan` |
| Tujuh pertanyaan tidak memblokir ingin ditutup lebih dulu | `/qv-grill` Amendment Pass |
| Salah satu SHA berubah | `/qv-trace` impact scan |
| Slice yang dihentikan ingin dibuka | Tutup Decision ID-nya lebih dulu lewat `/qv-grill`, lalu ulangi dari `requirement-completeness-gate` |

**Blueprint ini `MUST NOT` diteruskan ke `/qv-plan` sebelum empat pertanyaan memblokir terjawab.**
